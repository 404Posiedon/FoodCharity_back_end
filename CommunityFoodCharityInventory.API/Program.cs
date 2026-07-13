using CommunityFoodCharityInventory.API.Data;
using CommunityFoodCharityInventory.API.DTOs;
using CommunityFoodCharityInventory.API.Hubs;
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
//Add CORS later

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


// --- SignalR WebSockets Route Mapping ---
app.MapHub<DonorHub>("/donorHub");


app.Run();


