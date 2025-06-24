using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOCS.Order.Infrastucture.Migrations
{
    /// <inheritdoc />
    public partial class update_variant_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "VariantGroups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "VariantGroups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxSelect",
                table: "VariantGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinSelect",
                table: "VariantGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "VariantGroupItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VariantGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuItemVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariantGroupItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VariantGroupItems_MenuItemVariants_MenuItemVariantId",
                        column: x => x.MenuItemVariantId,
                        principalTable: "MenuItemVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VariantGroupItems_VariantGroups_VariantGroupId",
                        column: x => x.VariantGroupId,
                        principalTable: "VariantGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VariantGroupItems_MenuItemVariantId",
                table: "VariantGroupItems",
                column: "MenuItemVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_VariantGroupItems_VariantGroupId",
                table: "VariantGroupItems",
                column: "VariantGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VariantGroupItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "VariantGroups");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "VariantGroups");

            migrationBuilder.DropColumn(
                name: "MaxSelect",
                table: "VariantGroups");

            migrationBuilder.DropColumn(
                name: "MinSelect",
                table: "VariantGroups");

            migrationBuilder.AddColumn<Guid>(
                name: "MenuItemVariantId",
                table: "VariantGroups",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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
    }
}
