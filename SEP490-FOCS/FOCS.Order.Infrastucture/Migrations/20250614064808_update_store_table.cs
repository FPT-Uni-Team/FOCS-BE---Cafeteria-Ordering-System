using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOCS.Order.Infrastucture.Migrations
{
    /// <inheritdoc />
    public partial class update_store_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsOccupied",
                table: "Tables",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "logoUrl",
                table: "StoreSettings",
                newName: "LogoUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Tables",
                newName: "IsOccupied");

            migrationBuilder.RenameColumn(
                name: "LogoUrl",
                table: "StoreSettings",
                newName: "logoUrl");
        }
    }
}
