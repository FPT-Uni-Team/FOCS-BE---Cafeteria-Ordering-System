using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOCS.Order.Infrastucture.Migrations
{
    /// <inheritdoc />
    public partial class update_workshift_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StaffWorkshiftRegistrations_Workshifts_WorkshiftScheduleId",
                table: "StaffWorkshiftRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Workshifts_WorkshiftSchedules_WorkshiftId",
                table: "Workshifts");

            migrationBuilder.DropIndex(
                name: "IX_Workshifts_WorkshiftId",
                table: "Workshifts");

            migrationBuilder.DropColumn(
                name: "WorkDate",
                table: "WorkshiftSchedules");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Workshifts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Workshifts");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Workshifts");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Workshifts");

            migrationBuilder.DropColumn(
                name: "WorkshiftId",
                table: "Workshifts");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "WorkshiftSchedules",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "WorkshiftSchedules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "WorkshiftSchedules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "WorkshiftSchedules",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<Guid>(
                name: "WorkshiftId",
                table: "WorkshiftSchedules",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "WorkDate",
                table: "Workshifts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_WorkshiftSchedules_WorkshiftId",
                table: "WorkshiftSchedules",
                column: "WorkshiftId");

            migrationBuilder.AddForeignKey(
                name: "FK_StaffWorkshiftRegistrations_WorkshiftSchedules_WorkshiftScheduleId",
                table: "StaffWorkshiftRegistrations",
                column: "WorkshiftScheduleId",
                principalTable: "WorkshiftSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkshiftSchedules_Workshifts_WorkshiftId",
                table: "WorkshiftSchedules",
                column: "WorkshiftId",
                principalTable: "Workshifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StaffWorkshiftRegistrations_WorkshiftSchedules_WorkshiftScheduleId",
                table: "StaffWorkshiftRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkshiftSchedules_Workshifts_WorkshiftId",
                table: "WorkshiftSchedules");

            migrationBuilder.DropIndex(
                name: "IX_WorkshiftSchedules_WorkshiftId",
                table: "WorkshiftSchedules");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "WorkshiftSchedules");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "WorkshiftSchedules");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "WorkshiftSchedules");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "WorkshiftSchedules");

            migrationBuilder.DropColumn(
                name: "WorkshiftId",
                table: "WorkshiftSchedules");

            migrationBuilder.DropColumn(
                name: "WorkDate",
                table: "Workshifts");

            migrationBuilder.AddColumn<DateTime>(
                name: "WorkDate",
                table: "WorkshiftSchedules",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "Workshifts",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Workshifts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Workshifts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "Workshifts",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<Guid>(
                name: "WorkshiftId",
                table: "Workshifts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Workshifts_WorkshiftId",
                table: "Workshifts",
                column: "WorkshiftId");

            migrationBuilder.AddForeignKey(
                name: "FK_StaffWorkshiftRegistrations_Workshifts_WorkshiftScheduleId",
                table: "StaffWorkshiftRegistrations",
                column: "WorkshiftScheduleId",
                principalTable: "Workshifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workshifts_WorkshiftSchedules_WorkshiftId",
                table: "Workshifts",
                column: "WorkshiftId",
                principalTable: "WorkshiftSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
