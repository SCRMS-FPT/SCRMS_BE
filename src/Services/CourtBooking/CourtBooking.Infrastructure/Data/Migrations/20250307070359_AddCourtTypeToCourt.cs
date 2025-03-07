using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourtBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourtTypeToCourt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CourtType",
                table: "courts",
                type: "text",
                nullable: false,
                defaultValue: "Indoor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourtType",
                table: "courts");
        }
    }
}
