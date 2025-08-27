using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOCS.Order.Infrastucture.Migrations
{
    /// <inheritdoc />
    public partial class update_order_variants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemVariants_OrderDetails_OrderDetailId",
                table: "MenuItemVariants");

            migrationBuilder.DropIndex(
                name: "IX_MenuItemVariants_OrderDetailId",
                table: "MenuItemVariants");

            migrationBuilder.DropColumn(
                name: "OrderDetailId",
                table: "MenuItemVariants");

            migrationBuilder.AddColumn<string>(
                name: "Variants",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Variants",
                table: "OrderDetails");

            migrationBuilder.AddColumn<Guid>(
                name: "OrderDetailId",
                table: "MenuItemVariants",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemVariants_OrderDetailId",
                table: "MenuItemVariants",
                column: "OrderDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemVariants_OrderDetails_OrderDetailId",
                table: "MenuItemVariants",
                column: "OrderDetailId",
                principalTable: "OrderDetails",
                principalColumn: "Id");
        }
    }
}
