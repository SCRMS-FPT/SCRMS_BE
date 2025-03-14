using Coach.API.Data.Repositories;
using Coach.API.Data;
using BuildingBlocks.Messaging.Events;
using MassTransit;

namespace Coach.API.Consumers
{
    public class PaymentSucceededConsumer : IConsumer<PaymentSucceededEvent>
    {
        private readonly ICoachBookingRepository _bookingRepo;
        private readonly ICoachPackagePurchaseRepository _packagePurchaseRepo;
        private readonly CoachDbContext _context;

        public PaymentSucceededConsumer(
            ICoachBookingRepository bookingRepo,
            ICoachPackagePurchaseRepository packagePurchaseRepo,
            CoachDbContext context)
        {
            _bookingRepo = bookingRepo;
            _packagePurchaseRepo = packagePurchaseRepo;
            _context = context;
        }

        public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
        {
            var message = context.Message;

            // 1. Cập nhật trạng thái Booking
            var booking = await _bookingRepo.GetCoachBookingByIdAsync(
                        message.ReferenceId ?? throw new ArgumentNullException("Booking ID không tồn tại"),
                        context.CancellationToken);
            if (booking == null) throw new Exception("Booking not found");

            booking.Status = "confirmed";
            await _bookingRepo.UpdateCoachBookingAsync(booking ?? throw new ArgumentNullException("Booking ID không tồn tại"), context.CancellationToken);

            // 2. Xử lý Package Purchase (nếu dùng gói)
            if (booking.PackageId.HasValue)
            {
                var purchase = await _packagePurchaseRepo.GetCoachPackagePurchaseByIdAsync(booking.PackageId.Value, context.CancellationToken);
                if (purchase != null)
                {
                    purchase.SessionsUsed += 1;
                    await _packagePurchaseRepo.UpdateCoachPackagePurchaseAsync(purchase ?? throw new ArgumentNullException("Booking ID không tồn tại"), context.CancellationToken);
                }
            }

            await _context.SaveChangesAsync(); // Commit transaction
        }
    }
}