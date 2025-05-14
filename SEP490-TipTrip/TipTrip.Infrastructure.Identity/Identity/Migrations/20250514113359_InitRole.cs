using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TipTrip.Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class InitRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "CreatedBy", "Name", "NormalizedName", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { "c1d15070-8452-4e54-9c5f-a0d862669a72", "eaf0912f-a33b-474c-b3e3-094787ce697f", new DateTime(2025, 5, 14, 11, 33, 58, 13, DateTimeKind.Utc).AddTicks(6728), "System", "Admin", "ADMIN", new DateTime(2025, 5, 14, 11, 33, 58, 13, DateTimeKind.Utc).AddTicks(6728), "System" },
                    { "f80fd4aa-2c94-4ca9-8410-34043efa94bb", "f11d25b5-9040-4a96-ac78-ade564d1e9f1", new DateTime(2025, 5, 14, 11, 33, 58, 13, DateTimeKind.Utc).AddTicks(6728), "System", "Customer", "CUSTOMER", new DateTime(2025, 5, 14, 11, 33, 58, 13, DateTimeKind.Utc).AddTicks(6728), "System" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "c1d15070-8452-4e54-9c5f-a0d862669a72");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "f80fd4aa-2c94-4ca9-8410-34043efa94bb");
        }
    }
}
