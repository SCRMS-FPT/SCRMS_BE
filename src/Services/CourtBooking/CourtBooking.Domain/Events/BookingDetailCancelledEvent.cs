using System;

namespace CourtBooking.Domain.Events
{
    public record BookingDetailCancelledEvent(
        Guid BookingDetailId,
        Guid BookingId,
        Guid CourtId,
        TimeSpan StartTime,
        TimeSpan EndTime,
        string CancellationReason,
        DateTime CancellationTime
    );
}