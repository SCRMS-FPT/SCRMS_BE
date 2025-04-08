namespace BuildingBlocks.Messaging.Events
{
    public record BookingCancelledRefundEvent(
            Guid BookingId,
            Guid UserId,
            Guid SportCenterOwnerId,
            decimal RefundAmount,
            string CancellationReason,
            DateTime CancelledAt
        ) : IntegrationEvent;

    public record BookingCancelledNotificationEvent(
        Guid BookingId,
        Guid UserId,
        Guid SportCenterId,
        bool RefundProcessed,
        decimal RefundAmount,
        string CancellationReason,
        DateTime CancelledAt
    ) : IntegrationEvent;
}