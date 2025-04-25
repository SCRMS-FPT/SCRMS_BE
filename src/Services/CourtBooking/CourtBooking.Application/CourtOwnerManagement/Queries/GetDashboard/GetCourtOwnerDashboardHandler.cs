using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourtBooking.Application.Data;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CourtBooking.Application.CourtOwnerManagement.Queries.GetDashboard
{
    public class GetCourtOwnerDashboardHandler : IRequestHandler<GetCourtOwnerDashboardQuery, GetCourtOwnerDashboardResult>
    {
        private readonly IApplicationDbContext _context;
        private readonly ISportCenterRepository _sportCenterRepository;
        private readonly ICourtRepository _courtRepository;
        private readonly IBookingRepository _bookingRepository;

        public GetCourtOwnerDashboardHandler(
            IApplicationDbContext context,
            ISportCenterRepository sportCenterRepository,
            ICourtRepository courtRepository,
            IBookingRepository bookingRepository)
        {
            _context = context;
            _sportCenterRepository = sportCenterRepository;
            _courtRepository = courtRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<GetCourtOwnerDashboardResult> Handle(GetCourtOwnerDashboardQuery request, CancellationToken cancellationToken)
        {
            // Get all sport centers owned by the court owner
            var sportCenters = await _sportCenterRepository.GetSportCentersByOwnerIdAsync(request.OwnerId, cancellationToken);
            var sportCenterIds = sportCenters.Select(sc => sc.Id).ToList();

            // Get all courts in these sport centers
            var courts = await _courtRepository.GetCourtsBySportCenterIdsAsync(sportCenterIds, cancellationToken);
            var courtIds = courts.Select(c => c.Id).ToList();

            // Get today, start of week, start of month dates
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            // Get bookings statistics
            var allBookings = await GetBookingsForCourtIds(courtIds, null, null, cancellationToken);
            var todayBookings = await GetBookingsForCourtIds(courtIds, today, today, cancellationToken);
            var weekBookings = await GetBookingsForCourtIds(courtIds, startOfWeek, today, cancellationToken);
            var monthBookings = await GetBookingsForCourtIds(courtIds, startOfMonth, today, cancellationToken);

            // Previous periods for comparison
            var previousDayBookings = await GetBookingsForCourtIds(courtIds, today.AddDays(-1), today.AddDays(-1), cancellationToken);
            var previousWeekBookings = await GetBookingsForCourtIds(
                courtIds,
                startOfWeek.AddDays(-7),
                startOfWeek.AddDays(-1),
                cancellationToken);
            var previousMonthStart = startOfMonth.AddMonths(-1);
            var previousMonthEnd = startOfMonth.AddDays(-1);
            var previousMonthBookings = await GetBookingsForCourtIds(
                courtIds,
                previousMonthStart,
                previousMonthEnd,
                cancellationToken);

            // Calculate booking rates
            var totalBookings = allBookings.Count;
            var completedBookings = allBookings.Count(b => b.Status == BookingStatus.Completed);
            var confirmedBookings = allBookings.Count(b => b.Status == BookingStatus.Deposited); // Changed from Confirmed to Deposited

            var completedRate = totalBookings > 0 ? (decimal)completedBookings / totalBookings * 100 : 0;
            var confirmedRate = totalBookings > 0 ? (decimal)confirmedBookings / totalBookings * 100 : 0;

            // Calculate revenues
            var todayRevenue = CalculateRevenue(todayBookings);
            var weekRevenue = CalculateRevenue(weekBookings);
            var monthRevenue = CalculateRevenue(monthBookings);

            var previousDayRevenue = CalculateRevenue(previousDayBookings);
            var previousWeekRevenue = CalculateRevenue(previousWeekBookings);
            var previousMonthRevenue = CalculateRevenue(previousMonthBookings);

            // Get detailed data for today's bookings
            var todayBookingDetails = await GetDetailedBookingsForToday(courtIds, today, cancellationToken);

            return new GetCourtOwnerDashboardResult
            {
                TotalSportCenters = sportCenters.Count,
                TotalCourts = courts.Count,
                TotalBookings = totalBookings,
                CompletedBookingRate = completedRate,
                ConfirmedBookingRate = confirmedRate,
                TodayRevenue = CalculateRevenueStats(todayRevenue, previousDayRevenue),
                WeeklyRevenue = CalculateRevenueStats(weekRevenue, previousWeekRevenue),
                MonthlyRevenue = CalculateRevenueStats(monthRevenue, previousMonthRevenue),
                TodayBookings = todayBookingDetails
            };
        }

        private async Task<List<Domain.Models.Booking>> GetBookingsForCourtIds(
            List<CourtId> courtIds,
            DateTime? startDate,
            DateTime? endDate,
            CancellationToken cancellationToken)
        {
            var bookingsQuery = _context.Bookings.AsQueryable();

            // Filter by courts
            bookingsQuery = bookingsQuery.Where(b =>
                b.BookingDetails.Any(d =>
                    courtIds.Contains(d.CourtId)));

            // Filter by date
            if (startDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate >= startDate.Value);
            if (endDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate <= endDate.Value);

            return await bookingsQuery
                .Include(b => b.BookingDetails)
                .ToListAsync(cancellationToken);
        }

        private decimal CalculateRevenue(List<Domain.Models.Booking> bookings)
        {
            return bookings
                .Where(b => b.Status == BookingStatus.Completed || b.Status == BookingStatus.Deposited) // Changed from Confirmed to Deposited
                .Sum(b => b.TotalPrice);
        }

        private RevenueStats CalculateRevenueStats(decimal currentRevenue, decimal previousRevenue)
        {
            var percentageChange = previousRevenue > 0
                ? (currentRevenue - previousRevenue) / previousRevenue * 100
                : (currentRevenue > 0 ? 100 : 0);

            return new RevenueStats
            {
                Amount = currentRevenue,
                PercentageChange = Math.Abs(percentageChange),
                IsIncrease = currentRevenue >= previousRevenue
            };
        }

        private async Task<List<TodayBookingDto>> GetDetailedBookingsForToday(
            List<CourtId> courtIds,
            DateTime today,
            CancellationToken cancellationToken)
        {
            // Load bookings without user information
            var todayBookings = await _context.Bookings
                .Where(b =>
                    b.BookingDate.Date == today.Date &&
                    b.BookingDetails.Any(d => courtIds.Contains(d.CourtId)))
                .Include(b => b.BookingDetails)
                // Remove the .Include(b => b.User) to avoid loading user entities
                .ToListAsync(cancellationToken);

            // Get all court ids referenced in today's bookings
            var relevantCourtIds = todayBookings
                .SelectMany(b => b.BookingDetails.Select(d => d.CourtId))
                .Distinct()
                .ToList();

            // Get courts info
            var courtsInfo = await _context.Courts
                .Where(c => relevantCourtIds.Contains(c.Id))
                .Select(c => new
                {
                    CourtId = c.Id.Value,
                    CourtName = c.CourtName.Value,
                    SportCenterId = c.SportCenterId.Value
                })
                .ToListAsync(cancellationToken);

            // Get sport center names
            var sportCenterIds = courtsInfo.Select(c => SportCenterId.Of(c.SportCenterId)).Distinct().ToList();
            var sportCenters = await _context.SportCenters
                .Where(sc => sportCenterIds.Contains(sc.Id))
                .Select(sc => new { SportCenterId = sc.Id.Value, SportCenterName = sc.Name })
                .ToDictionaryAsync(sc => sc.SportCenterId, sc => sc.SportCenterName, cancellationToken);

            // Lookup dictionary for courts
            var courtLookup = courtsInfo.ToDictionary(
                c => c.CourtId,
                c => new { c.CourtName, c.SportCenterId }
            );

            // Create detailed booking information
            var result = new List<TodayBookingDto>();
            foreach (var booking in todayBookings)
            {
                foreach (var detail in booking.BookingDetails)
                {
                    var courtInfo = courtLookup.GetValueOrDefault(detail.CourtId.Value);

                    if (courtInfo != null && sportCenters.TryGetValue(courtInfo.SportCenterId, out var sportCenterName))
                    {
                        result.Add(new TodayBookingDto
                        {
                            BookingId = booking.Id.Value,
                            UserId = booking.UserId.Value, // Use UserId instead of CustomerName
                            CourtName = courtInfo.CourtName,
                            SportCenterName = sportCenterName,
                            StartTime = detail.StartTime.ToString(@"hh\:mm"),
                            EndTime = detail.EndTime.ToString(@"hh\:mm"),
                            TotalPrice = detail.TotalPrice,
                            Status = booking.Status.ToString()
                        });
                    }
                }
            }

            return result.OrderBy(b => TimeSpan.Parse(b.StartTime)).ToList();
        }

    }
}