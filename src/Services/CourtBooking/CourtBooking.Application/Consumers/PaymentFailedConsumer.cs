using BuildingBlocks.Messaging.Events;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.ValueObjects;
using MassTransit;
using System.Threading.Tasks;

namespace CourtBooking.Application.Consumers
{
    public class PaymentFailedConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<PaymentFailedConsumer> _logger;

        public PaymentFailedConsumer(
            IBookingRepository bookingRepository,
            ILogger<PaymentFailedConsumer> logger)
        {
            _bookingRepository = bookingRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var message = context.Message;
            _logger.LogWarning("Payment failed for booking: {BookingId}, reason: {Reason}",
                message.ReferenceId, message.Description);

            if (!message.ReferenceId.HasValue)
            {
                _logger.LogWarning("Payment failure event received without booking ID");
                return;
            }

            try
            {
                var bookingId = BookingId.Of(message.ReferenceId.Value);
                var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, CancellationToken.None);

                if (booking == null)
                {
                    _logger.LogWarning("Booking not found: {BookingId}", bookingId.Value);
                    return;
                }

                // Only handle if booking is in pending payment state
                if (booking.Status == BookingStatus.PendingPayment)
                {
                    // Set the booking back to pending or cancel it depending on requirements
                    booking.UpdateStatus(BookingStatus.PaymentFail);
                    booking.SetCancellationReason("Payment failed: " + message.Description);

                    await _bookingRepository.UpdateBookingAsync(booking, CancellationToken.None);
                    _logger.LogInformation("Booking {BookingId} status updated to {Status} due to payment failure",
                        bookingId.Value, booking.Status);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment failure for booking {BookingId}",
                    message.ReferenceId);
                // Consider a retry mechanism or dead-letter queue
            }
        }
    }
}