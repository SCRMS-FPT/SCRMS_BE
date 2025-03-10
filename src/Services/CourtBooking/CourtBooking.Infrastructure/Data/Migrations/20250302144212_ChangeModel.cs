using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourtBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sport_centers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    Images_Avatar = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Images_ImageUrls = table.Column<string>(type: "jsonb", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Address_AddressLine = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Address_City = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Address_Commune = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Address_District = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LocationPoint_Latitude = table.Column<double>(type: "DOUBLE PRECISION", nullable: false),
                    LocationPoint_Longitude = table.Column<double>(type: "DOUBLE PRECISION", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sport_centers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Icon = table.Column<string>(type: "TEXT", nullable: false),
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
                    SportCenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    SportId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlotDuration = table.Column<double>(type: "double precision", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Facilities = table.Column<string>(type: "JSONB", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Open"),
                    CourtName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_courts_sport_centers_SportCenterId",
                        column: x => x.SportCenterId,
                        principalTable: "sport_centers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "court_schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourtId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int[]>(type: "integer[]", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "TIME", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "TIME", nullable: false),
                    PriceSlot = table.Column<decimal>(type: "DECIMAL", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_court_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_court_schedules_courts_CourtId",
                        column: x => x.CourtId,
                        principalTable: "courts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_court_schedules_CourtId",
                table: "court_schedules",
                column: "CourtId");

            migrationBuilder.CreateIndex(
                name: "IX_courts_SportCenterId",
                table: "courts",
                column: "SportCenterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "court_schedules");

            migrationBuilder.DropTable(
                name: "sports");

            migrationBuilder.DropTable(
                name: "courts");

            migrationBuilder.DropTable(
                name: "sport_centers");
        }
    }
}
