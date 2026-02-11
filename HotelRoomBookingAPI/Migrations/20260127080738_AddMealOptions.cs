using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelRoomBookingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddMealOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientKindId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasBreakfast",
                table: "BookingOccupants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasDinner",
                table: "BookingOccupants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasLunch",
                table: "BookingOccupants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ClientKinds",
                columns: table => new
                {
                    ClientKindId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientKindName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientKinds", x => x.ClientKindId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ClientKindId",
                table: "Bookings",
                column: "ClientKindId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_ClientKinds_ClientKindId",
                table: "Bookings",
                column: "ClientKindId",
                principalTable: "ClientKinds",
                principalColumn: "ClientKindId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_ClientKinds_ClientKindId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "ClientKinds");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ClientKindId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ClientKindId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "HasBreakfast",
                table: "BookingOccupants");

            migrationBuilder.DropColumn(
                name: "HasDinner",
                table: "BookingOccupants");

            migrationBuilder.DropColumn(
                name: "HasLunch",
                table: "BookingOccupants");
        }
    }
}
