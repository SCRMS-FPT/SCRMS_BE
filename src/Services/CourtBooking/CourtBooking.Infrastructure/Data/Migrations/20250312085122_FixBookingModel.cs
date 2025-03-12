using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourtBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixBookingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookings_courts_CourtId",
                table: "bookings");

            migrationBuilder.DropTable(
                name: "booking_prices");

            migrationBuilder.DropIndex(
                name: "IX_bookings_CourtId",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "CourtId",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "bookings");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "bookings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalTime",
                table: "bookings",
                type: "DECIMAL",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "booking_details",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourtId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "TIME", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "TIME", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "DECIMAL", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_booking_details_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_booking_details_BookingId",
                table: "booking_details",
                column: "BookingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_details");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "TotalTime",
                table: "bookings");

            migrationBuilder.AddColumn<Guid>(
                name: "CourtId",
                table: "bookings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "bookings",
                type: "TIME",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "bookings",
                type: "TIME",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.CreateTable(
                name: "booking_prices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "TIME", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Price = table.Column<decimal>(type: "DECIMAL", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "TIME", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_prices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_booking_prices_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_CourtId",
                table: "bookings",
                column: "CourtId");

            migrationBuilder.CreateIndex(
                name: "IX_booking_prices_BookingId",
                table: "booking_prices",
                column: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_bookings_courts_CourtId",
                table: "bookings",
                column: "CourtId",
                principalTable: "courts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
