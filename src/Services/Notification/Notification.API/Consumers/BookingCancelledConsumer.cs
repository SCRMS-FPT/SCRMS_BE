using BuildingBlocks.Messaging.Events;
using MassTransit;
using Notification.API.Services;

namespace Notification.API.Consumers
{
    public class BookingCancelledConsumer : IConsumer<BookingCancelledNotificationEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<BookingCancelledConsumer> _logger;

        public BookingCancelledConsumer(
            INotificationService notificationService,
            ILogger<BookingCancelledConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingCancelledNotificationEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation($"Processing notification for cancelled booking {message.BookingId}");

            // Notify the user about cancellation and refund status
            string userMessage = message.RefundProcessed
                ? $"Your booking #{message.BookingId} has been cancelled. A refund of {message.RefundAmount:C} has been processed to your wallet."
                : $"Your booking #{message.BookingId} has been cancelled. No refund is applicable based on the cancellation policy.";

            await _notificationService.SendToOwner(
                message.UserId,
                "Booking Cancellation");

            // Notify the sport center owner
            await _notificationService.SendToOwner(
                message.SportCenterId,
                $"Booking #{message.BookingId} has been cancelled by user. Reason: {message.CancellationReason}",
                new Dictionary<string, string>
                {
                { "bookingId", message.BookingId.ToString() },
                { "userId", message.UserId.ToString() },
                { "cancelledAt", message.CancelledAt.ToString("yyyy-MM-dd HH:mm:ss") },
                { "refundAmount", message.RefundAmount.ToString() }
                });
        }
    }
}