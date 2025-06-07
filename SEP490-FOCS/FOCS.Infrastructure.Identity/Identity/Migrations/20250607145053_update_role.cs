using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FOCS.Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class update_role : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "cf0a1168-7764-4a16-8e7e-f69c834ba3fe");

            migrationBuilder.AlterColumn<Guid>(
                name: "StoreId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "35e0de77-c3a0-4a6e-ae26-d7fa0721ed62", "35e0de77-c3a0-4a6e-ae26-d7fa0721ed62", "Manager", "MANAGER" },
                    { "bc722046-068e-47ca-b280-c9088e3ebc1f", "bc722046-068e-47ca-b280-c9088e3ebc1f", "Staff", "STAFF" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "35e0de77-c3a0-4a6e-ae26-d7fa0721ed62");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: "bc722046-068e-47ca-b280-c9088e3ebc1f");

            migrationBuilder.AlterColumn<Guid>(
                name: "StoreId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "cf0a1168-7764-4a16-8e7e-f69c834ba3fe", "cf0a1168-7764-4a16-8e7e-f69c834ba3fe", "Customer", "CUSTOMER" });
        }
    }
}
