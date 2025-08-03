using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOCS.Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class setup_focs_points : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FOCSPoint",
                table: "Users",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FOCSPoint",
                table: "Users");
        }
    }
}
