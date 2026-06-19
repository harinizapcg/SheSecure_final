using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SheSecure.Safety_WellnessService.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestDateToWellnessRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RequestDate",
                table: "WellnessRequests",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestDate",
                table: "WellnessRequests");
        }
    }
}
