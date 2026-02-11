using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelRoomBookingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingOccupant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF OBJECT_ID('BookingOccupants', 'U') IS NOT NULL DROP TABLE BookingOccupants");

            migrationBuilder.CreateTable(
                name: "BookingOccupants",
                columns: table => new
                {
                    BookingOccupantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    AadhaarEncrypted = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    AadhaarLast4 = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    AadhaarHash = table.Column<byte[]>(type: "varbinary(900)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingOccupants", x => x.BookingOccupantId);
                    table.ForeignKey(
                        name: "FK_BookingOccupants_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingOccupants_BookingId",
                table: "BookingOccupants",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "UX_BookingOccupants_AadhaarHash",
                table: "BookingOccupants",
                column: "AadhaarHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingOccupants");
        }
    }
}
