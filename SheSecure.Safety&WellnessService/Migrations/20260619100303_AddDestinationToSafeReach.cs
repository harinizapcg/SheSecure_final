using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SheSecure.Safety_WellnessService.Migrations
{
    /// <inheritdoc />
    public partial class AddDestinationToSafeReach : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DestinationLatitude",
                table: "SafeReachChecks",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DestinationLongitude",
                table: "SafeReachChecks",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinationLatitude",
                table: "SafeReachChecks");

            migrationBuilder.DropColumn(
                name: "DestinationLongitude",
                table: "SafeReachChecks");
        }
    }
}
