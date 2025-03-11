namespace BuildingBlocks.Messaging.Events
{
    public record PaymentSucceededEvent(
        Guid TransactionId,
        Guid UserId,
        decimal Amount,
        DateTime Timestamp,
        string Description);

    public record RefundProcessedEvent(
        Guid TransactionId,
        Guid UserId,
        decimal Amount,
        DateTime Timestamp,
        string Reason);
}