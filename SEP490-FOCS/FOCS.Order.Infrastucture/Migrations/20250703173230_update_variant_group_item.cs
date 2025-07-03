using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOCS.Order.Infrastucture.Migrations
{
    /// <inheritdoc />
    public partial class update_variant_group_item : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrepPerTime",
                table: "MenuItemVariants");

            migrationBuilder.DropColumn(
                name: "QuantityPerTime",
                table: "MenuItemVariants");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "MenuItemVariantGroupItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PrepPerTime",
                table: "MenuItemVariantGroupItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantityPerTime",
                table: "MenuItemVariantGroupItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "MenuItemVariantGroupItems");

            migrationBuilder.DropColumn(
                name: "PrepPerTime",
                table: "MenuItemVariantGroupItems");

            migrationBuilder.DropColumn(
                name: "QuantityPerTime",
                table: "MenuItemVariantGroupItems");

            migrationBuilder.AddColumn<int>(
                name: "PrepPerTime",
                table: "MenuItemVariants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantityPerTime",
                table: "MenuItemVariants",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
