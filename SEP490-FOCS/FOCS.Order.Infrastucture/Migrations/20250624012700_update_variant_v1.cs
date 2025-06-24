using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOCS.Order.Infrastucture.Migrations
{
    /// <inheritdoc />
    public partial class update_variant_v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemVariants_VariantGroups_VariantGroupId",
                table: "MenuItemVariants");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Promotions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "VariantGroupId",
                table: "MenuItemVariants",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemVariants_VariantGroups_VariantGroupId",
                table: "MenuItemVariants",
                column: "VariantGroupId",
                principalTable: "VariantGroups",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemVariants_VariantGroups_VariantGroupId",
                table: "MenuItemVariants");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Promotions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "VariantGroupId",
                table: "MenuItemVariants",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemVariants_VariantGroups_VariantGroupId",
                table: "MenuItemVariants",
                column: "VariantGroupId",
                principalTable: "VariantGroups",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
