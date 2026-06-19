using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SheSecure.Safety_WellnessService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddResolutionNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinationLatitude",
                table: "SafeReachChecks");

            migrationBuilder.DropColumn(
                name: "DestinationLongitude",
                table: "SafeReachChecks");

            migrationBuilder.AddColumn<string>(
                name: "ResolutionNotes",
                table: "EmergencyAlerts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResolutionNotes",
                table: "EmergencyAlerts");

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
    }
}
