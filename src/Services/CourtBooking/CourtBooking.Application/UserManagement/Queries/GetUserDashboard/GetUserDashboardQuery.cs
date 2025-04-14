using System;
using System.Collections.Generic;
using MediatR;
using CourtBooking.Domain.Enums;

namespace CourtBooking.Application.UserManagement.Queries.GetUserDashboard
{
    public record GetUserDashboardQuery(Guid UserId) : IRequest<GetUserDashboardResult>;

    public class GetUserDashboardResult
    {
        // Upcoming bookings
        public List<UpcomingBookingDto> UpcomingBookings { get; set; }

        // Incomplete transactions (bookings with remaining balance > 0)
        public List<IncompleteTransactionDto> IncompleteTransactions { get; set; }

        // Statistics summary
        public UserBookingStatsDto Stats { get; set; }
    }

    public class UpcomingBookingDto
    {
        public Guid BookingId { get; set; }
        public DateTime BookingDate { get; set; }
        public string SportCenterName { get; set; }
        public string CourtName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
    }

    public class IncompleteTransactionDto
    {
        public Guid BookingId { get; set; }
        public DateTime BookingDate { get; set; }
        public string SportCenterName { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public string Status { get; set; }
    }

    public class UserBookingStatsDto
    {
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int UpcomingBookings { get; set; }
    }
}