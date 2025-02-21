using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Coach.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Coaches",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SportId = table.Column<int>(type: "integer", nullable: false),
                    Bio = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RatePerHour = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coaches", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "CoachBookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    SportId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachBookings_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachSchedules_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoachBookings_CoachId",
                table: "CoachBookings",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachSchedules_CoachId",
                table: "CoachSchedules",
                column: "CoachId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoachBookings");

            migrationBuilder.DropTable(
                name: "CoachSchedules");

            migrationBuilder.DropTable(
                name: "Coaches");
        }
    }
}
