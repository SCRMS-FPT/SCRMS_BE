using System;

namespace CourtBooking.Domain.Events
{
    public record BookingCancelledEvent(
        Guid BookingId,
        Guid UserId,
        string CancellationReason,
        DateTime CancellationTime
    );
}