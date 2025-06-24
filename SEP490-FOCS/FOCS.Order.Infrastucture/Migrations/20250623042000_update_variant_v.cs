using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOCS.Order.Infrastucture.Migrations
{
    /// <inheritdoc />
    public partial class update_variant_v : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VariantGroupItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "VariantGroups");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "VariantGroups",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "VariantGroups",
                newName: "id");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "VariantGroups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "VariantGroupItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuItemVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VariantGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
    }
}
