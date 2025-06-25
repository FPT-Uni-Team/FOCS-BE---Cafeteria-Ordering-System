using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOCS.Order.Infrastucture.Migrations
{
    /// <inheritdoc />
    public partial class update_variant_v4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VariantGroups_MenuItems_MenuItemId",
                table: "VariantGroups");

            migrationBuilder.DropIndex(
                name: "IX_VariantGroups_MenuItemId",
                table: "VariantGroups");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "VariantGroups");

            migrationBuilder.DropColumn(
                name: "MaxSelect",
                table: "VariantGroups");

            migrationBuilder.DropColumn(
                name: "MenuItemId",
                table: "VariantGroups");

            migrationBuilder.DropColumn(
                name: "MinSelect",
                table: "VariantGroups");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "VariantGroups",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "VariantGroups",
                newName: "Id");

            migrationBuilder.CreateTable(
                name: "MenuItemVariantGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VariantGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinSelect = table.Column<int>(type: "int", nullable: false),
                    MaxSelect = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemVariantGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItemVariantGroups_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuItemVariantGroups_VariantGroups_VariantGroupId",
                        column: x => x.VariantGroupId,
                        principalTable: "VariantGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuItemVariantGroupItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuItemVariantGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuItemVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemVariantGroupItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItemVariantGroupItems_MenuItemVariantGroups_MenuItemVariantGroupId",
                        column: x => x.MenuItemVariantGroupId,
                        principalTable: "MenuItemVariantGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuItemVariantGroupItems_MenuItemVariants_MenuItemVariantId",
                        column: x => x.MenuItemVariantId,
                        principalTable: "MenuItemVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemVariantGroupItems_MenuItemVariantGroupId",
                table: "MenuItemVariantGroupItems",
                column: "MenuItemVariantGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemVariantGroupItems_MenuItemVariantId",
                table: "MenuItemVariantGroupItems",
                column: "MenuItemVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemVariantGroups_MenuItemId",
                table: "MenuItemVariantGroups",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemVariantGroups_VariantGroupId",
                table: "MenuItemVariantGroups",
                column: "VariantGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuItemVariantGroupItems");

            migrationBuilder.DropTable(
                name: "MenuItemVariantGroups");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "VariantGroups",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "VariantGroups",
                newName: "id");

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

            migrationBuilder.AddColumn<Guid>(
                name: "MenuItemId",
                table: "VariantGroups",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "MinSelect",
                table: "VariantGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_VariantGroups_MenuItemId",
                table: "VariantGroups",
                column: "MenuItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_VariantGroups_MenuItems_MenuItemId",
                table: "VariantGroups",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
