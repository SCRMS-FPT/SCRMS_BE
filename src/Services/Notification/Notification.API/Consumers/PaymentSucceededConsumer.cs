using BuildingBlocks.Messaging.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Notification.API.Data.Model;
using Notification.API.Hubs;
using Notification.API.Services;

namespace Notification.API.Consumers
{
    public class PaymentSucceededConsumer : IConsumer<PaymentSucceededEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotifyHub> _hubContext;
        private readonly ILogger<PaymentSucceededConsumer> _logger;

        public PaymentSucceededConsumer(
            INotificationService notificationService,
            IHubContext<NotifyHub> hubContext,
            ILogger<PaymentSucceededConsumer> logger)
        {
            _notificationService = notificationService;
            _hubContext = hubContext;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
        {
            var paymentEvent = context.Message;

            // Log that we received the event
            _logger.LogInformation(
                "Đã nhận PaymentSucceededEvent: TransactionId={TransactionId}, UserId={UserId}, Amount={Amount}",
                paymentEvent.TransactionId, paymentEvent.UserId, paymentEvent.Amount);

            // Xác định loại thông báo dựa vào mô tả hoặc loại thanh toán
            string title = "Thanh toán thành công";
            string content = $"Bạn đã thanh toán thành công số tiền {paymentEvent.Amount:N0} VND vào lúc {paymentEvent.Timestamp:HH:mm dd/MM/yyyy}. {paymentEvent.Description}";

            // Nếu là nạp tiền vào ví
            if (paymentEvent.Description?.Contains("nạp tiền") == true ||
                paymentEvent.Description?.Contains("ví") == true)
            {
                title = "Nạp tiền vào ví thành công";
                content = $"Số tiền {paymentEvent.Amount:N0} VND đã được nạp vào ví thành công vào lúc {paymentEvent.Timestamp:HH:mm dd/MM/yyyy}.";
            }

            // Create notification entity
            var noti = new MessageNotification
            {
                Receiver = paymentEvent.UserId,
                Title = title,
                Content = content,
                Type = "payment",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            // Send payment confirmation notification via the notification service
            await _notificationService.SendPaymentConfirmation(
                paymentEvent.UserId,
                paymentEvent.Amount,
                paymentEvent.Description
            );
            _logger.LogInformation("Đã lưu thông báo thanh toán cho người dùng {UserId}", paymentEvent.UserId);

            // Additionally notify through SignalR hub if connected
            await _hubContext.Clients.User(paymentEvent.UserId.ToString())
                .SendAsync("ReceiveNotification", noti);
            _logger.LogInformation("Đã gửi thông báo thanh toán qua SignalR cho người dùng {UserId}", paymentEvent.UserId);
        }

    }
}