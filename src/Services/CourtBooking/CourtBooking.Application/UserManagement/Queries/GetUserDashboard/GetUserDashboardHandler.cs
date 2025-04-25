using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourtBooking.Application.Data;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CourtBooking.Application.UserManagement.Queries.GetUserDashboard
{
    public class GetUserDashboardHandler : IRequestHandler<GetUserDashboardQuery, GetUserDashboardResult>
    {
        private readonly IApplicationDbContext _context;

        public GetUserDashboardHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GetUserDashboardResult> Handle(GetUserDashboardQuery request, CancellationToken cancellationToken)
        {
            var userId = UserId.Of(request.UserId);
            var today = DateTime.Today;

            // Get upcoming bookings (limit to next 5 upcoming bookings)
            var upcomingBookings = await GetUpcomingBookingsAsync(userId, today, 5, cancellationToken);

            // Get incomplete transactions (remaining balance > 0)
            var incompleteTransactions = await GetIncompleteTransactionsAsync(userId, cancellationToken);

            // Get booking statistics
            var stats = await GetUserBookingStatsAsync(userId, today, cancellationToken);

            return new GetUserDashboardResult
            {
                UpcomingBookings = upcomingBookings,
                IncompleteTransactions = incompleteTransactions,
                Stats = stats
            };
        }

        private async Task<List<UpcomingBookingDto>> GetUpcomingBookingsAsync(UserId userId, DateTime today, int limit, CancellationToken cancellationToken)
        {
            // Get upcoming bookings that haven't been cancelled/rejected/failed payment
            var upcomingBookingsQuery = _context.Bookings
                .Include(b => b.BookingDetails)
                .Where(b => b.UserId == userId)
                .Where(b => b.BookingDate >= today)
                .Where(b => b.Status == BookingStatus.Deposited || b.Status == BookingStatus.PendingPayment) // Changed from Confirmed to Deposited
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.BookingDetails.Min(d => d.StartTime))
                .Take(limit);

            var bookings = await upcomingBookingsQuery.ToListAsync(cancellationToken);

            // Get all court IDs used in bookings
            var courtIds = bookings
                .SelectMany(b => b.BookingDetails.Select(d => d.CourtId))
                .Distinct()
                .ToList();

            // Get courts info
            var courtsInfo = await _context.Courts
                .Where(c => courtIds.Contains(c.Id))
                .Select(c => new
                {
                    CourtId = c.Id,
                    CourtName = c.CourtName.Value,
                    SportCenterId = c.SportCenterId
                })
                .ToDictionaryAsync(c => c.CourtId, c => new { c.CourtName, c.SportCenterId }, cancellationToken);

            // Get sport center names
            var sportCenterIds = courtsInfo.Values.Select(c => c.SportCenterId).Distinct().ToList();
            var sportCenters = await _context.SportCenters
                .Where(sc => sportCenterIds.Contains(sc.Id))
                .Select(sc => new { SportCenterId = sc.Id, SportCenterName = sc.Name })
                .ToDictionaryAsync(sc => sc.SportCenterId, sc => sc.SportCenterName, cancellationToken);

            // Map to DTOs
            var upcomingBookingDtos = new List<UpcomingBookingDto>();

            foreach (var booking in bookings)
            {
                foreach (var detail in booking.BookingDetails)
                {
                    if (courtsInfo.TryGetValue(detail.CourtId, out var courtInfo) &&
                        sportCenters.TryGetValue(courtInfo.SportCenterId, out var sportCenterName))
                    {
                        upcomingBookingDtos.Add(new UpcomingBookingDto
                        {
                            BookingId = booking.Id.Value,
                            BookingDate = booking.BookingDate,
                            CourtName = courtInfo.CourtName,
                            SportCenterName = sportCenterName,
                            StartTime = detail.StartTime,
                            EndTime = detail.EndTime,
                            TotalPrice = detail.TotalPrice,
                            Status = booking.Status.ToString()
                        });
                    }
                }
            }

            return upcomingBookingDtos.OrderBy(b => b.BookingDate).ThenBy(b => b.StartTime).Take(limit).ToList();
        }

        private async Task<List<IncompleteTransactionDto>> GetIncompleteTransactionsAsync(UserId userId, CancellationToken cancellationToken)
        {
            // Find bookings with remaining balance > 0
            var bookingsWithBalance = await _context.Bookings
                .Include(b => b.BookingDetails)
                .Where(b => b.UserId == userId)
                .Where(b => b.RemainingBalance > 0)
                .Where(b => b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Completed && b.Status != BookingStatus.PaymentFail)
                .OrderByDescending(b => b.BookingDate)
                .Take(5)
                .ToListAsync(cancellationToken);

            var result = new List<IncompleteTransactionDto>();

            // Get sport center IDs from all booking details
            var courtIds = bookingsWithBalance
                .SelectMany(b => b.BookingDetails.Select(d => d.CourtId))
                .Distinct()
                .ToList();

            // Get courts with their sport center IDs
            var courtSportCenters = await _context.Courts
                .Where(c => courtIds.Contains(c.Id))
                .Select(c => new { CourtId = c.Id, SportCenterId = c.SportCenterId })
                .ToDictionaryAsync(c => c.CourtId, c => c.SportCenterId, cancellationToken);

            // Get sport center names
            var sportCenterIds = courtSportCenters.Values.Distinct().ToList();
            var sportCenterNames = await _context.SportCenters
                .Where(sc => sportCenterIds.Contains(sc.Id))
                .Select(sc => new { SportCenterId = sc.Id, Name = sc.Name })
                .ToDictionaryAsync(sc => sc.SportCenterId, sc => sc.Name, cancellationToken);

            // Map to DTOs
            foreach (var booking in bookingsWithBalance)
            {
                // Get the sport center name for this booking
                var sportCenterName = "Unknown";
                if (booking.BookingDetails.Any())
                {
                    var firstCourtId = booking.BookingDetails.First().CourtId;
                    if (courtSportCenters.TryGetValue(firstCourtId, out var sportCenterId) &&
                        sportCenterNames.TryGetValue(sportCenterId, out var name))
                    {
                        sportCenterName = name;
                    }
                }

                result.Add(new IncompleteTransactionDto
                {
                    BookingId = booking.Id.Value,
                    BookingDate = booking.BookingDate,
                    SportCenterName = sportCenterName,
                    TotalPrice = booking.TotalPrice,
                    PaidAmount = booking.TotalPrice - booking.RemainingBalance,
                    RemainingBalance = booking.RemainingBalance,
                    Status = booking.Status.ToString()
                });
            }

            return result;
        }

        private async Task<UserBookingStatsDto> GetUserBookingStatsAsync(UserId userId, DateTime today, CancellationToken cancellationToken)
        {
            // Get counts of bookings by status
            var bookingCounts = await _context.Bookings
                .Where(b => b.UserId == userId)
                .GroupBy(b => 1)
                .Select(g => new
                {
                    TotalCount = g.Count(),
                    CompletedCount = g.Count(b => b.Status == BookingStatus.Completed),
                    CancelledCount = g.Count(b => b.Status == BookingStatus.Cancelled),
                    UpcomingCount = g.Count(b => b.BookingDate >= today &&
                        (b.Status == BookingStatus.Deposited || b.Status == BookingStatus.PendingPayment)) // Changed from Confirmed to Deposited
                })
                .FirstOrDefaultAsync(cancellationToken);

            return new UserBookingStatsDto
            {
                TotalBookings = bookingCounts?.TotalCount ?? 0,
                CompletedBookings = bookingCounts?.CompletedCount ?? 0,
                CancelledBookings = bookingCounts?.CancelledCount ?? 0,
                UpcomingBookings = bookingCounts?.UpcomingCount ?? 0
            };
        }
    }
}