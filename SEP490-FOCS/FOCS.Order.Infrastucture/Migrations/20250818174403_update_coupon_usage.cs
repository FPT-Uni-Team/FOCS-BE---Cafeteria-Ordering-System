using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOCS.Order.Infrastucture.Migrations
{
    /// <inheritdoc />
    public partial class update_coupon_usage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WorkshiftId",
                table: "StaffWorkshiftRegistrations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_StaffWorkshiftRegistrations_WorkshiftId",
                table: "StaffWorkshiftRegistrations",
                column: "WorkshiftId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_CouponId",
                table: "CouponUsages",
                column: "CouponId");

            migrationBuilder.AddForeignKey(
                name: "FK_CouponUsages_Coupons_CouponId",
                table: "CouponUsages",
                column: "CouponId",
                principalTable: "Coupons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffWorkshiftRegistrations_Workshifts_WorkshiftId",
                table: "StaffWorkshiftRegistrations",
                column: "WorkshiftId",
                principalTable: "Workshifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CouponUsages_Coupons_CouponId",
                table: "CouponUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffWorkshiftRegistrations_Workshifts_WorkshiftId",
                table: "StaffWorkshiftRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_StaffWorkshiftRegistrations_WorkshiftId",
                table: "StaffWorkshiftRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_CouponUsages_CouponId",
                table: "CouponUsages");

            migrationBuilder.DropColumn(
                name: "WorkshiftId",
                table: "StaffWorkshiftRegistrations");
        }
    }
}
