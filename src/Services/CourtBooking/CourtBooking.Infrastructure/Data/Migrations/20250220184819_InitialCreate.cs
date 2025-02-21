using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourtBooking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "courts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SportId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Facilities = table.Column<string>(type: "JSON", nullable: false),
                    PricePerHour = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Open"),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CourtName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Location_Address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Location_City = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Location_Commune = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Location_District = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_courts_sports_SportId",
                        column: x => x.SportId,
                        principalTable: "sports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "court_operating_hours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourtId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    CloseTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_court_operating_hours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_court_operating_hours_courts_CourtId",
                        column: x => x.CourtId,
                        principalTable: "courts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_court_operating_hours_CourtId",
                table: "court_operating_hours",
                column: "CourtId");

            migrationBuilder.CreateIndex(
                name: "IX_courts_SportId",
                table: "courts",
                column: "SportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "court_operating_hours");

            migrationBuilder.DropTable(
                name: "courts");

            migrationBuilder.DropTable(
                name: "sports");
        }
    }
}
