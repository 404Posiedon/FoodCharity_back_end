using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunityFoodCharityInventory.API.Migrations
{
    /// <inheritdoc />
    public partial class intialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FoodInventry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentQuantity = table.Column<double>(type: "float", nullable: false),
                    PledgedQuantity = table.Column<double>(type: "float", nullable: false),
                    TargetCap = table.Column<double>(type: "float", nullable: false),
                    MinimumThreshold = table.Column<double>(type: "float", nullable: false),
                    MaximumThreshold = table.Column<double>(type: "float", nullable: false),
                    CriticalThreshold = table.Column<double>(type: "float", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodInventry", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoodInventry");
        }
    }
}
