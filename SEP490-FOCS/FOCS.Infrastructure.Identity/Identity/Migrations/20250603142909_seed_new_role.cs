using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FOCS.Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class seed_new_role : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "123dc5a4-2ce6-4497-9c3a-f614752c28fa", "123dc5a4-2ce6-4497-9c3a-f614752c28fa", "User", "USER" },
                    { "5961f48a-d700-4807-bc3d-4fa9d229e9fb", "5961f48a-d700-4807-bc3d-4fa9d229e9fb", "KitchenStaff", "KITCHENSTAFF" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "123dc5a4-2ce6-4497-9c3a-f614752c28fa");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "5961f48a-d700-4807-bc3d-4fa9d229e9fb");
        }
    }
}
