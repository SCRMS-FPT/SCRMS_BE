namespace BuildingBlocks.Messaging.Events
{
    // Event cơ sở chứa các thuộc tính chung
    public abstract record PaymentBaseEvent(
        Guid TransactionId,
        Guid UserId,
        decimal Amount,
        DateTime Timestamp,
        string Description) : IntegrationEvent;

    // Event thanh toán cho gói dịch vụ (Identity Service)
    public record ServicePackagePaymentEvent(
        Guid TransactionId,
        Guid UserId,
        decimal Amount,
        DateTime Timestamp,
        string Description,
        string PackageType,
        DateTime ValidUntil) : PaymentBaseEvent(TransactionId, UserId, Amount, Timestamp, Description);

    // Event thanh toán cho Coach
    public record CoachPaymentEvent(
        Guid TransactionId,
        Guid UserId,
        Guid CoachId,
        decimal Amount,
        DateTime Timestamp,
        string Description,
        Guid? BookingId,
        Guid? PackageId) : PaymentBaseEvent(TransactionId, UserId, Amount, Timestamp, Description);

    // Giữ lại event gốc cho khả năng tương thích ngược
    public record PaymentSucceededEvent(
        Guid TransactionId,
        Guid UserId,
        Guid? ReferenceId,
        decimal Amount,
        DateTime Timestamp,
        string Description,
        string PaymentType) : PaymentBaseEvent(TransactionId, UserId, Amount, Timestamp, Description);

    public record RefundProcessedEvent(
        Guid TransactionId,
        Guid UserId,
        decimal Amount,
        DateTime Timestamp,
        string Reason) : IntegrationEvent;
}