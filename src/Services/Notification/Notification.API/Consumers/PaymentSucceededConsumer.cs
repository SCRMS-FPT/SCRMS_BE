using MassTransit;
using Notification.API.Services;
using BuildingBlocks.Messaging.Events;

namespace Notification.API.Consumers
{
    public class PaymentSucceededConsumer : IConsumer<PaymentSucceededEvent>
    {
        private readonly INotificationService _notificationService;

        public PaymentSucceededConsumer(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
        {
            var message = context.Message;
            await _notificationService.SendPaymentConfirmation(
                message.UserId,
                message.Amount,
                message.Description);
        }
    }
}