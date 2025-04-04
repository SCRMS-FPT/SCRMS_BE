using BuildingBlocks.Messaging.Events;
using MassTransit;
using Notification.API.Services;

namespace Notification.API.Consumers
{
    public class SendMailConsumer : IConsumer<SendMailEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<SendMailConsumer> _logger;
        public SendMailConsumer(
            INotificationService notificationService,
            ILogger<SendMailConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<SendMailEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation($"Processing send email to {message.To}");

            await _notificationService.SendMail(message.To, message.Subject, message.Body,message.isHtml);
        }
    }
}
