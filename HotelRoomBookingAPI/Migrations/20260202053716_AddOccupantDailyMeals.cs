using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelRoomBookingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddOccupantDailyMeals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OccupantDailyMeals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingOccupantId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HasBreakfast = table.Column<bool>(type: "bit", nullable: false),
                    HasLunch = table.Column<bool>(type: "bit", nullable: false),
                    HasDinner = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OccupantDailyMeals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OccupantDailyMeals_BookingOccupants_BookingOccupantId",
                        column: x => x.BookingOccupantId,
                        principalTable: "BookingOccupants",
                        principalColumn: "BookingOccupantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OccupantDailyMeals_BookingOccupantId",
                table: "OccupantDailyMeals",
                column: "BookingOccupantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OccupantDailyMeals");
        }
    }
}
