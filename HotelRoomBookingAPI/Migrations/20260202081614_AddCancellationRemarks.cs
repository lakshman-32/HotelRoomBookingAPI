using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelRoomBookingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCancellationRemarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationRemarks",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationRemarks",
                table: "Bookings");
        }
    }
}
