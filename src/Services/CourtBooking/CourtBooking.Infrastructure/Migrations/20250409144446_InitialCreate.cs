using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourtBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "DATE", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalTime = table.Column<decimal>(type: "DECIMAL", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "DECIMAL", nullable: false),
                    RemainingBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    InitialDeposit = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPaid = table.Column<decimal>(type: "numeric", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    CancellationReason = table.Column<string>(type: "text", nullable: true),
                    CancellationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

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
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    CourtType = table.Column<string>(type: "text", nullable: false, defaultValue: "Indoor"),
                    MinDepositPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    CancellationWindowHours = table.Column<int>(type: "integer", nullable: false, defaultValue: 24),
                    RefundPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
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
                    table.ForeignKey(
                        name: "FK_courts_sports_SportId",
                        column: x => x.SportId,
                        principalTable: "sports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    table.ForeignKey(
                        name: "FK_booking_details_courts_CourtId",
                        column: x => x.CourtId,
                        principalTable: "courts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "court_promotions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourtId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DiscountType = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "DECIMAL", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "DATE", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "DATE", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_court_promotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_court_promotions_courts_CourtId",
                        column: x => x.CourtId,
                        principalTable: "courts",
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
                name: "IX_booking_details_BookingId",
                table: "booking_details",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_booking_details_CourtId",
                table: "booking_details",
                column: "CourtId");

            migrationBuilder.CreateIndex(
                name: "IX_court_promotions_CourtId",
                table: "court_promotions",
                column: "CourtId");

            migrationBuilder.CreateIndex(
                name: "IX_court_schedules_CourtId_DayOfWeek",
                table: "court_schedules",
                columns: new[] { "CourtId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_courts_SportCenterId",
                table: "courts",
                column: "SportCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_courts_SportId",
                table: "courts",
                column: "SportId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedAt",
                table: "OutboxMessages",
                column: "ProcessedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_details");

            migrationBuilder.DropTable(
                name: "court_promotions");

            migrationBuilder.DropTable(
                name: "court_schedules");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "courts");

            migrationBuilder.DropTable(
                name: "sport_centers");

            migrationBuilder.DropTable(
                name: "sports");
        }
    }
}
