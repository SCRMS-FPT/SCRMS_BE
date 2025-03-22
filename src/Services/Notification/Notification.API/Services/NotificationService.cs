using Microsoft.AspNetCore.SignalR;
using Notification.API.Data;
using Notification.API.Hubs;
using Notification.API.Data.Model;
using Microsoft.Extensions.Logging;

namespace Notification.API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly NotificationDbContext _context;
        private readonly IHubContext<NotifyHub> _hubContext;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            NotificationDbContext context,
            IHubContext<NotifyHub> hubContext,
            IEmailService emailService, ILogger<NotificationService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendPaymentConfirmation(
            Guid userId,
            decimal amount,
            string description)
        {
            var title = "Payment Confirmed";
            var content = $"Your payment of {amount:N2} for {description} has been processed successfully.";

            // Lưu vào database
            var notification = new MessageNotification
            {
                Receiver = userId,
                Title = title,
                Content = content,
                Type = "payment"
            };

            _context.MessageNotifications.Add(notification);
            await _context.SaveChangesAsync();

            // Gửi real-time qua SignalR
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("ReceiveNotification", notification);

            // Gửi email (optional)
            var userEmail = await GetUserEmail(userId);
            if (!string.IsNullOrEmpty(userEmail))
            {
                await _emailService.SendEmailAsync(
                    userEmail,
                    title,
                    content,
                    isHtml: false);
            }
        }

        private async Task<string> GetUserEmail(Guid userId)
        {
            // Triển khai logic lấy email từ Identity Service
            // Ví dụ: gọi API hoặc database
            return "user@example.com";
        }

        public async Task SendPushNotification(
                Guid userId,
                string title,
                string message,
                Dictionary<string, string>? data = null)
        {
            // Implement logic gửi notification thực tế ở đây
            _logger.LogInformation($"Sending push to {userId}: {title} - {message}");

            // Ví dụ tích hợp Firebase Cloud Messaging
            /*
            var fcmMessage = new Message()
            {
                Token = "device_token",
                Notification = new Notification
                {
                    Title = title,
                    Body = message
                },
                Data = data
            };
            await FirebaseMessaging.DefaultInstance.SendAsync(fcmMessage);
            */

            await Task.CompletedTask;
        }

        public async Task SendToOwner(
            Guid ownerId,
            string message,
            Dictionary<string, string>? data = null)
        {
            // Logic gửi thông báo cho chủ sân/coach
            _logger.LogInformation($"Notifying owner {ownerId}: {message}");
            await Task.CompletedTask;
        }

        public Task SendRefundNotification(Guid userId, decimal amount, string reason)
        {
            throw new NotImplementedException();
        }
    }
}