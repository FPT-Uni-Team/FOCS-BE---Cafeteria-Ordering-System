using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOCS.Order.Infrastucture.Migrations
{
    /// <inheritdoc />
    public partial class update_variant_group : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemVariants_VariantGroups_VariantGroupId",
                table: "MenuItemVariants");

            migrationBuilder.DropIndex(
                name: "IX_MenuItemVariants_VariantGroupId",
                table: "MenuItemVariants");

            migrationBuilder.DropColumn(
                name: "VariantGroupId",
                table: "MenuItemVariants");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "VariantGroups",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "VariantGroups",
                newName: "Id");

            migrationBuilder.AddColumn<Guid>(
                name: "MenuItemVariantId",
                table: "VariantGroups",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<double>(
                name: "MaxDiscountValue",
                table: "Promotions",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.CreateIndex(
                name: "IX_VariantGroups_MenuItemVariantId",
                table: "VariantGroups",
                column: "MenuItemVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_VariantGroups_MenuItemVariants_MenuItemVariantId",
                table: "VariantGroups",
                column: "MenuItemVariantId",
                principalTable: "MenuItemVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VariantGroups_MenuItemVariants_MenuItemVariantId",
                table: "VariantGroups");

            migrationBuilder.DropIndex(
                name: "IX_VariantGroups_MenuItemVariantId",
                table: "VariantGroups");

            migrationBuilder.DropColumn(
                name: "MenuItemVariantId",
                table: "VariantGroups");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "VariantGroups",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "VariantGroups",
                newName: "id");

            migrationBuilder.AlterColumn<double>(
                name: "MaxDiscountValue",
                table: "Promotions",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VariantGroupId",
                table: "MenuItemVariants",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemVariants_VariantGroupId",
                table: "MenuItemVariants",
                column: "VariantGroupId");

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
