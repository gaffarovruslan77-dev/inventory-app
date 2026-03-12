using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryApp.Web.Migrations
{
    public partial class AddItemCustomIdIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Items_InventoryId_CustomId",
                table: "Items",
                columns: new[] { "InventoryId", "CustomId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_InventoryId_CustomId",
                table: "Items");
        }
    }
}

