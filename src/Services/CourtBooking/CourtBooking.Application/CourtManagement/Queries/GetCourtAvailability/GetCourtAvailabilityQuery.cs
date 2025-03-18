using System;
using System.Collections.Generic;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourtAvailability
{
    public record GetCourtAvailabilityQuery(
        Guid CourtId,
        DateTime StartDate,
        DateTime EndDate
    ) : IRequest<GetCourtAvailabilityResult>;

    public record GetCourtAvailabilityResult(
        Guid CourtId,
        int SlotDuration,
        List<DailySchedule> Schedule
    );

    public record DailySchedule(
        DateTime Date,
        int DayOfWeek,
        List<TimeSlot> TimeSlots
    );

    public record TimeSlot(
        string StartTime,
        string EndTime,
        decimal Price,
        string Status,
        PromotionInfo? Promotion = null,
        string? BookedBy = null
    );

    public record PromotionInfo(
        string DiscountType,
        decimal DiscountValue
    );
}