using System;
using System.Threading.Tasks;
using BuildingBlocks.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Domain.Enums;

namespace CourtBooking.Application.Consumers
{
    public class PaymentSucceededConsumer : IConsumer<PaymentSucceededEvent>
    {
        private readonly ILogger<PaymentSucceededConsumer> _logger;
        private readonly IBookingRepository _bookingRepository;
        private readonly IApplicationDbContext _dbContext;

        public PaymentSucceededConsumer(
            ILogger<PaymentSucceededConsumer> logger,
            IBookingRepository bookingRepository,
            IApplicationDbContext dbContext)
        {
            _logger = logger;
            _bookingRepository = bookingRepository;
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
        {
            var paymentEvent = context.Message;

            _logger.LogInformation(
                "Đã nhận PaymentSucceededEvent: TransactionId = {TransactionId}, UserId = {UserId}, Amount = {Amount}",
                paymentEvent.TransactionId, paymentEvent.UserId, paymentEvent.Amount);

            // Xử lý dựa trên loại thanh toán
            if (paymentEvent.PaymentType == "CourtBooking" && paymentEvent.ReferenceId.HasValue)
            {
                var bookingId = BookingId.Of(paymentEvent.ReferenceId.Value);
                var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, context.CancellationToken);

                if (booking != null)
                {
                    // Cập nhật trạng thái đặt sân thành "Đã thanh toán"
                    booking.Confirm();
                    await _bookingRepository.UpdateBookingAsync(booking, context.CancellationToken);

                    _logger.LogInformation("Đã cập nhật trạng thái đặt sân thành Đã thanh toán cho BookingId: {BookingId}", bookingId.Value);
                }
                else
                {
                    _logger.LogWarning("Không tìm thấy đặt sân với Id: {BookingId}", bookingId.Value);
                }
            }

            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }
    }
}