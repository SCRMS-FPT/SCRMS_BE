using System;

namespace BuildingBlocks.Messaging.Events
{
    public record CoachBookingCancelledRefundEvent(
        Guid BookingId,
        Guid UserId,
        Guid CoachId,
        decimal RefundAmount,
        string CancellationReason,
        DateTime CancelledAt
    ) : IntegrationEvent;

    public record CoachBookingCancelledNotificationEvent(
        Guid BookingId,
        Guid UserId,
        Guid CoachId,
        bool RefundProcessed,
        decimal RefundAmount,
        string CancellationReason,
        DateTime CancelledAt
    ) : IntegrationEvent;
}
