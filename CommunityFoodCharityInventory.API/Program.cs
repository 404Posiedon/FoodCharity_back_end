using CommunityFoodCharityInventory.API.Data;
using CommunityFoodCharityInventory.API.DTOs;
using CommunityFoodCharityInventory.API.Hubs;
using CommunityFoodCharityInventory.Domain.Models;
using Microsoft.AspNetCore.Http.HttpResults;
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
//Add CORS later for front-end access

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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
//1. Get Inventory Items
//

app.MapGet("/api/inventory", async (CharityDbContext db) =>
{
    var items = await db.FoodInventry.ToListAsync();
    var dtos = items.Select(i => new InventoryItemDto(i.Id, i.Name, i.EffectiveQuantity, i.Status.ToString()));
    return Results.Ok(dtos);
})
  .WithName("GetInventoryItems")
  .WithSummary("Retrieves current live kitchen stock amounts and dynamic urgency metrics.");

//2 Create new inventory item
app.MapPost("/api/inventory", async (CreateItemRequestDto request, CharityDbContext db) =>
{
    var newItem = new InventoryItem(
        Guid.NewGuid(),
        request.Name,
        request.CurrentQuantity,
        request.TargetCap,
        request.MinThreshold,
        request.CritThreshold);

    db.FoodInventry.Add(newItem);
    await db.SaveChangesAsync();

    return Results.Created($"/api/inventory/{newItem.Id}", new InventoryItemDto(
         newItem.Id, newItem.Name, newItem.EffectiveQuantity, newItem.Status.ToString()
     ));
})
.WithName("CreateInventoryItem")
.WithSummary("Administratively registers a new trackable item type into the system.");


// --- SignalR WebSockets Route Mapping ---
app.MapHub<DonorHub>("/donorHub");


app.Run();


