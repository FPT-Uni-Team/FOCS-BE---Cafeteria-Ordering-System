using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOCS.Order.Infrastucture.Migrations
{
    /// <inheritdoc />
    public partial class update_coupon_field : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinimumItemQuantity",
                table: "Coupons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MinimumOrderAmount",
                table: "Coupons",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimumItemQuantity",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "MinimumOrderAmount",
                table: "Coupons");
        }
    }
}
