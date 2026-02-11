using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelRoomBookingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddOnRequestMealFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBreakfastOnRequest",
                table: "OccupantDailyMeals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDinnerOnRequest",
                table: "OccupantDailyMeals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLunchOnRequest",
                table: "OccupantDailyMeals",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBreakfastOnRequest",
                table: "OccupantDailyMeals");

            migrationBuilder.DropColumn(
                name: "IsDinnerOnRequest",
                table: "OccupantDailyMeals");

            migrationBuilder.DropColumn(
                name: "IsLunchOnRequest",
                table: "OccupantDailyMeals");
        }
    }
}
