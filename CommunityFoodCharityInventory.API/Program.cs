using CommunityFoodCharityInventory.API.Data;
using CommunityFoodCharityInventory.API.DTOs;
using CommunityFoodCharityInventory.API.Hubs;
using CommunityFoodCharityInventory.Domain.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//Add Services
builder.Services.AddDbContext<CharityDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddSignalR();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Food Charity Live Inventory API");
    });
}

// Minimal HTTP API Endpoints

// 1. Get Inventory Items
app.MapGet("/api/inventory", async (CharityDbContext db) =>
{
    var items = await db.FoodInventry.ToListAsync();

    // FIXED: Swapped TargetCap and EffectiveQuantity to match your DTO order
    var dtos = items.Select(i => new InventoryItemDto(
        i.Id,
        i.Name,
        i.MinimumThreshold,
        i.MaximumThreshold,
        i.CurrentQuantity,
        i.EffectiveQuantity,
        i.TargetCap,
        i.Status.ToString()
    ));
    return Results.Ok(dtos);
})
.WithName("GetInventoryItems")
.WithSummary("Retrieves current live kitchen stock amounts and dynamic urgency metrics.");

// 2. Create new inventory item
app.MapPost("/api/inventory", async (CreateItemRequestDto request, CharityDbContext db) =>
{
    var newItem = new InventoryItem(
        Guid.NewGuid(),
        request.Name,
        request.CurrentQuantity,
        request.TargetCap,
        request.MinThreshold,
        request.MaxThreshold,
        request.CritThreshold);
        
    db.FoodInventry.Add(newItem);
    await db.SaveChangesAsync();

    return Results.Created($"/api/inventory/{newItem.Id}", new InventoryItemRequestDto(
         newItem.Id, newItem.Name, newItem.EffectiveQuantity,newItem.MinimumThreshold,newItem.MaximumThreshold,newItem.TargetCap, newItem.Status.ToString()
     ));
})
.WithName("CreateInventoryItem")
.WithSummary("Administratively registers a new trackable item type into the system.");

// 3. Pledge Stock
app.MapPost("/api/inventory/{id}/pledge", async (
    Guid id,
    PledgeRequestDto request,
    CharityDbContext db,
    IHubContext<DonorHub, IDonorHubClient> hubContext) =>
{
    var item = await db.FoodInventry.FindAsync(id);
    if (item == null) return Results.NotFound("Inventory item not found.");

    if (!item.TryAddPledge(request.Quantity))
    {
        return Results.BadRequest("Thank you! However, other pledges have already maxed out our target storage capacity.");
    }

    try
    {
        await db.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict("The inventory status changed while processing your request. Please try again.");
    }

    
    await hubContext.Clients.All.ReceiveInventoryUpdate(new InventoryItemDto(
        item.Id, item.Name, item.MinimumThreshold, item.MaximumThreshold, item.CurrentQuantity, item.EffectiveQuantity, item.TargetCap, item.Status.ToString()
    ));

    return Results.Ok(new { Message = "Pledge registered successfully!" });
})
.WithName("CreatePledge")
.WithSummary("Registers a donor drop-off commitment and updates the live web dashboards.");

// 4. Fulfill Pledge (Pledge Arrives physically at the kitchen)
app.MapPost("/api/inventory/{id}/fulfill-pledge", async (
    Guid id,
    PledgeRequestDto request,
    CharityDbContext db,
    IHubContext<DonorHub, IDonorHubClient> hubContext) =>
{
    var item = await db.FoodInventry.FindAsync(id);
    if (item == null) return Results.NotFound();

    item.ReleasePledge(request.Quantity);
    item.AdjustQuantity(request.Quantity);

    await db.SaveChangesAsync();

    
    await hubContext.Clients.All.ReceiveInventoryUpdate(new InventoryItemDto(
        item.Id, item.Name, item.MinimumThreshold, item.MaximumThreshold, item.CurrentQuantity, item.EffectiveQuantity, item.TargetCap, item.Status.ToString()
    ));

    return Results.Ok(new { Message = "Pledge physically received and stock updated." });
})
.WithName("FulfillPledge")
.WithSummary("Converts virtual pledged quantities into true physical stock upon physical arrival.");

// 5. Cancel / Revoke Pledge
app.MapPost("/api/inventory/{id}/cancel-pledge", async (
    Guid id,
    PledgeRequestDto request,
    CharityDbContext db,
    IHubContext<DonorHub, IDonorHubClient> hubContext) =>
{
    var item = await db.FoodInventry.FindAsync(id);
    if (item == null) return Results.NotFound();

    item.ReleasePledge(request.Quantity);
    await db.SaveChangesAsync();

    
    await hubContext.Clients.All.ReceiveInventoryUpdate(new InventoryItemDto(
        item.Id, item.Name, item.MinimumThreshold, item.MaximumThreshold, item.CurrentQuantity, item.EffectiveQuantity, item.TargetCap, item.Status.ToString()
    ));

    return Results.Ok(new { Message = "Pledge cancelled. Capacity freed for other donors." });
})
.WithName("CancelPledge")
.WithSummary("Manually drops a reservation tier allocation if a donor falls through.");

// 6. Direct Restock (Bulk unexpected arrivals/Purchased goods)
app.MapPost("/api/inventory/{id}/restock", async (
    Guid id,
    RestockRequestDto request,
    CharityDbContext db,
    IHubContext<DonorHub, IDonorHubClient> hubContext) =>
{
    var item = await db.FoodInventry.FindAsync(id);
    if (item == null) return Results.NotFound();

    item.AdjustQuantity(request.Quantity);
    await db.SaveChangesAsync();

   
    await hubContext.Clients.All.ReceiveInventoryUpdate(new InventoryItemDto(
        item.Id, item.Name, item.MinimumThreshold, item.MaximumThreshold, item.CurrentQuantity, item.EffectiveQuantity, item.TargetCap, item.Status.ToString()
    ));

    return Results.Ok(new { Message = "Direct stock replenishment successful." });
})
.WithName("RestockStock")
.WithSummary("Directly increments physical storage volumes without a preceding pledge trail.");

// 7. Deduct Stock (Kitchen usage)
app.MapPost("/api/inventory/{id}/deduct", async (
    Guid id,
    DeductRequestDto request,
    CharityDbContext db,
    IHubContext<DonorHub, IDonorHubClient> hubContext) =>
{
    var item = await db.FoodInventry.FindAsync(id);
    if (item == null) return Results.NotFound();

    item.AdjustQuantity(-request.Quantity);
    await db.SaveChangesAsync();

    await hubContext.Clients.All.ReceiveInventoryUpdate(new InventoryItemDto(
       item.Id, item.Name, item.MinimumThreshold, item.MaximumThreshold, item.CurrentQuantity, item.EffectiveQuantity, item.TargetCap, item.Status.ToString()
   ));

    return Results.Ok(new { Message = "Stock deducted successfully." });
})
.WithName("DeductStock")
.WithSummary("Deducts physical stock items when used by kitchen personnel.");


// --- SignalR WebSocket Route Mapping ---
app.MapHub<DonorHub>("/donorHub");

app.Run();


