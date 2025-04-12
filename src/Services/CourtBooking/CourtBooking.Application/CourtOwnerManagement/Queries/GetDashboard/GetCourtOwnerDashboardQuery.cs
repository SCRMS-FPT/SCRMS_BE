using System;
using System.Collections.Generic;
using MediatR;

namespace CourtBooking.Application.CourtOwnerManagement.Queries.GetDashboard
{
    public record GetCourtOwnerDashboardQuery(Guid OwnerId) : IRequest<GetCourtOwnerDashboardResult>;

    public class GetCourtOwnerDashboardResult
    {
        // Summary statistics
        public int TotalSportCenters { get; set; }
        public int TotalCourts { get; set; }
        public int TotalBookings { get; set; }
        public decimal CompletedBookingRate { get; set; }
        public decimal ConfirmedBookingRate { get; set; }

        // Revenue statistics
        public RevenueStats TodayRevenue { get; set; }
        public RevenueStats WeeklyRevenue { get; set; }
        public RevenueStats MonthlyRevenue { get; set; }

        // Today's bookings
        public List<TodayBookingDto> TodayBookings { get; set; }
    }

    public class RevenueStats
    {
        public decimal Amount { get; set; }
        public decimal PercentageChange { get; set; }
        public bool IsIncrease { get; set; }
    }

    public class TodayBookingDto
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public string CourtName { get; set; }
        public string SportCenterName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
    }
}