using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelRoomBookingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddOccupantTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CheckInTime",
                table: "BookingOccupants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckOutTime",
                table: "BookingOccupants",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "BookingOccupants");

            migrationBuilder.DropColumn(
                name: "CheckOutTime",
                table: "BookingOccupants");
        }
    }
}
