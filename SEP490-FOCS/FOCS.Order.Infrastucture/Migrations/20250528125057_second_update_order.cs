using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOCS.Order.Infrastucture.Migrations
{
    /// <inheritdoc />
    public partial class second_update_order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CouponUsages_User_UserId1",
                table: "CouponUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_User_UserId1",
                table: "Feedbacks");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_UserId1",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_CouponUsages_UserId1",
                table: "CouponUsages");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "CouponUsages");

            migrationBuilder.AddColumn<Guid>(
                name: "OrderWrapId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderWrapId",
                table: "Orders",
                column: "OrderWrapId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderWraps_OrderWrapId",
                table: "Orders",
                column: "OrderWrapId",
                principalTable: "OrderWraps",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderWraps_OrderWrapId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderWrapId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderWrapId",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Feedbacks",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "CouponUsages",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_UserId1",
                table: "Feedbacks",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_UserId1",
                table: "CouponUsages",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CouponUsages_User_UserId1",
                table: "CouponUsages",
                column: "UserId1",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_User_UserId1",
                table: "Feedbacks",
                column: "UserId1",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}
