namespace Notification.API.Services
{
    public interface INotificationService
    {
        Task SendPaymentConfirmation(
            Guid userId,
            decimal amount,
            string description);

        Task SendRefundNotification(
            Guid userId,
            decimal amount,
            string reason);
    }
}