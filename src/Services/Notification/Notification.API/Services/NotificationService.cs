using Microsoft.AspNetCore.SignalR;
using Notification.API.Data;
using Notification.API.Hubs;
using Notification.API.Data.Model;

namespace Notification.API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly NotificationDbContext _context;
        private readonly IHubContext<NotifyHub> _hubContext;
        private readonly IEmailService _emailService;

        public NotificationService(
            NotificationDbContext context,
            IHubContext<NotifyHub> hubContext,
            IEmailService emailService)
        {
            _context = context;
            _hubContext = hubContext;
            _emailService = emailService;
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

        public Task SendRefundNotification(Guid userId, decimal amount, string reason)
        {
            throw new NotImplementedException();
        }

        private async Task<string> GetUserEmail(Guid userId)
        {
            // Triển khai logic lấy email từ Identity Service
            // Ví dụ: gọi API hoặc database
            return "user@example.com";
        }
    }
}