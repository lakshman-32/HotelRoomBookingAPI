using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelRoomBookingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddMealCancellationFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBreakfastCancelled",
                table: "OccupantDailyMeals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDinnerCancelled",
                table: "OccupantDailyMeals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLunchCancelled",
                table: "OccupantDailyMeals",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBreakfastCancelled",
                table: "OccupantDailyMeals");

            migrationBuilder.DropColumn(
                name: "IsDinnerCancelled",
                table: "OccupantDailyMeals");

            migrationBuilder.DropColumn(
                name: "IsLunchCancelled",
                table: "OccupantDailyMeals");
        }
    }
}
