using CommunityFoodCharityInventory.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CommunityFoodCharityInventory.API.Data
{
    public class CharityDbContext:DbContext
    {
        //Constructor
        public CharityDbContext(DbContextOptions<CharityDbContext> options) : base(options) { }

        public DbSet<InventoryItem> FoodInventry => Set<InventoryItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // EF Core mapping setup for the private properties in your Domain Model
            modelBuilder.Entity<InventoryItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();

                // Native EF Core Optimistic Concurrency Token handling
                entity.Property(e => e.RowVersion)
                      .IsRowVersion();
            });


        }
       
    }
}
