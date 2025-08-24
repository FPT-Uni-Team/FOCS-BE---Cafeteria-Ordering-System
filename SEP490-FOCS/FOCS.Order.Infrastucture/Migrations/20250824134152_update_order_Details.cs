using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOCS.Order.Infrastucture.Migrations
{
    /// <inheritdoc />
    public partial class update_order_Details : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_MenuItemVariants_VariantId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_VariantId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "VariantId",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<Guid>(
                name: "VariantId",
                table: "OrderDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_VariantId",
                table: "OrderDetails",
                column: "VariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_MenuItemVariants_VariantId",
                table: "OrderDetails",
                column: "VariantId",
                principalTable: "MenuItemVariants",
                principalColumn: "Id");
        }
    }
}
