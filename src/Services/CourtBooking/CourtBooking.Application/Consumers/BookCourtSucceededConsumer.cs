using BuildingBlocks.Messaging.Events;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.ValueObjects;
using MassTransit;
using System.Threading.Tasks;

namespace CourtBooking.Application.Consumers
{
    public class BookCourtSucceededConsumer : IConsumer<BookCourtSucceededEvent>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<BookCourtSucceededConsumer> _logger;

        public BookCourtSucceededConsumer(
            IBookingRepository bookingRepository,
            ILogger<BookCourtSucceededConsumer> logger)
        {
            _bookingRepository = bookingRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookCourtSucceededEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Payment succeeded for booking: {BookingId}", message.ReferenceId);

            if (!message.ReferenceId.HasValue)
            {
                _logger.LogWarning("Payment success event received without booking ID");
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

                // Only update if booking is in pending payment state
                if (booking.Status == BookingStatus.PendingPayment)
                {
                    if (Enum.TryParse<BookingStatus>(message.StatusBook, out var status))
                    {
                        booking.UpdateStatus(status);
                    }
                    else if (booking.RemainingBalance == 0)
                    {
                        booking.UpdateStatus(BookingStatus.Completed);
                    }
                    else
                    {
                        booking.UpdateStatus(BookingStatus.Confirmed);
                    }

                    await _bookingRepository.UpdateBookingAsync(booking, CancellationToken.None);
                    _logger.LogInformation("Booking {BookingId} status updated to {Status}",
                        bookingId.Value, booking.Status);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment success for booking {BookingId}",
                    message.ReferenceId);
                // Consider a retry mechanism or dead-letter queue
            }
        }
    }
}