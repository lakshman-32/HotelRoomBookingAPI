using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelRoomBookingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoomStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    RoomCleaned = table.Column<bool>(type: "bit", nullable: false),
                    BathroomCleaned = table.Column<bool>(type: "bit", nullable: false),
                    BedCleaned = table.Column<bool>(type: "bit", nullable: false),
                    WaterBottlesProvided = table.Column<bool>(type: "bit", nullable: false),
                    BedsheetProvided = table.Column<bool>(type: "bit", nullable: false),
                    TowelProvided = table.Column<bool>(type: "bit", nullable: false),
                    DustbinCleaned = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomStatuses_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomStatuses_BookingId",
                table: "RoomStatuses",
                column: "BookingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomStatuses");
        }
    }
}
