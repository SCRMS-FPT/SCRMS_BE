using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourtBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixBookingModelv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PromotionId",
                table: "bookings");

            migrationBuilder.CreateIndex(
                name: "IX_booking_details_CourtId",
                table: "booking_details",
                column: "CourtId");

            migrationBuilder.AddForeignKey(
                name: "FK_booking_details_courts_CourtId",
                table: "booking_details",
                column: "CourtId",
                principalTable: "courts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_booking_details_courts_CourtId",
                table: "booking_details");

            migrationBuilder.DropIndex(
                name: "IX_booking_details_CourtId",
                table: "booking_details");

            migrationBuilder.AddColumn<Guid>(
                name: "PromotionId",
                table: "bookings",
                type: "uuid",
                nullable: true);
        }
    }
}
