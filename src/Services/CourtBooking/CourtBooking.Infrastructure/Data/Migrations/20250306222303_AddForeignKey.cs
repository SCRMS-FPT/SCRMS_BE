using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourtBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_courts_SportId",
                table: "courts",
                column: "SportId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_CourtId",
                table: "bookings",
                column: "CourtId");

            migrationBuilder.AddForeignKey(
                name: "FK_bookings_courts_CourtId",
                table: "bookings",
                column: "CourtId",
                principalTable: "courts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_courts_sports_SportId",
                table: "courts",
                column: "SportId",
                principalTable: "sports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookings_courts_CourtId",
                table: "bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_courts_sports_SportId",
                table: "courts");

            migrationBuilder.DropIndex(
                name: "IX_courts_SportId",
                table: "courts");

            migrationBuilder.DropIndex(
                name: "IX_bookings_CourtId",
                table: "bookings");
        }
    }
}
