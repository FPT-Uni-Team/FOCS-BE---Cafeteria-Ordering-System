using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOCS.Order.Infrastucture.Migrations
{
    /// <inheritdoc />
    public partial class update_promotion_attribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CountUsed",
                table: "Promotions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxUsage",
                table: "Promotions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxUsagePerUser",
                table: "Promotions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimumItemQuantity",
                table: "Promotions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MinimumOrderAmount",
                table: "Promotions",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountUsed",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "MaxUsage",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "MaxUsagePerUser",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "MinimumItemQuantity",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "MinimumOrderAmount",
                table: "Promotions");
        }
    }
}
