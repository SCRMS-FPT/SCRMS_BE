using BuildingBlocks.Messaging.Events;
using CourtBooking.Application.Data;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.ValueObjects;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CourtBooking.Application.Consumers
{
    public class BookCourtSucceededConsumer : IConsumer<BookCourtSucceededEvent>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<BookCourtSucceededConsumer> _logger;
        private readonly IApplicationDbContext _dbContext;

        public BookCourtSucceededConsumer(
            IBookingRepository bookingRepository,
            IApplicationDbContext dbContext,
            ILogger<BookCourtSucceededConsumer> logger)
        {
            _bookingRepository = bookingRepository;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookCourtSucceededEvent> context)
        {
            var paymentEvent = context.Message;

            _logger.LogInformation(
                "Đã nhận BookCourtSucceededEvent: TransactionId = {TransactionId}, UserId = {UserId}, Amount = {Amount}, PaymentType = {PaymentType}",
                paymentEvent.TransactionId, paymentEvent.UserId, paymentEvent.Amount, paymentEvent.PaymentType);

            if (!paymentEvent.ReferenceId.HasValue)
            {
                _logger.LogWarning("Booking ID không có trong event");
                return;
            }

            var bookingId = BookingId.Of(paymentEvent.ReferenceId.Value);
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, context.CancellationToken);

            if (booking == null)
            {
                _logger.LogWarning("Không tìm thấy đặt sân với Id: {BookingId}", bookingId.Value);
                return;
            }

            // Check if this is an initial payment or an additional payment
            if (paymentEvent.PaymentType == "CourtBookingAdditional")
            {
                // Process additional payment through domain method
                decimal oldRemainingBalance = booking.RemainingBalance;

                // Use domain method to process payment instead of direct property assignment
                booking.ProcessAdditionalPayment(paymentEvent.Amount);

                _logger.LogInformation(
                    "Cập nhật thanh toán bổ sung cho BookingId: {BookingId}, số dư ban đầu: {OldBalance}, số dư mới: {NewBalance}",
                    bookingId.Value, oldRemainingBalance, booking.RemainingBalance);
            }
            else if (booking.Status == BookingStatus.PendingPayment)
            {
                // Initial payment - use domain methods to change status
                if (Enum.TryParse<BookingStatus>(paymentEvent.StatusBook, out var status))
                {
                    booking.UpdateStatus(status);
                }
                else if (booking.RemainingBalance == 0)
                {
                    booking.UpdateStatus(BookingStatus.Completed);
                }
                else
                {
                    booking.UpdateStatus(BookingStatus.Deposited);
                }

                _logger.LogInformation("Đã cập nhật trạng thái đặt sân thành {Status} cho BookingId: {BookingId}",
                    booking.Status, bookingId.Value);
            }

            await _bookingRepository.UpdateBookingAsync(booking, context.CancellationToken);
            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }
    }
}