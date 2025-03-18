using BuildingBlocks.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Coach.API.Consumers
{
    public class CoachPaymentConsumer : IConsumer<PaymentSucceededEvent>, IConsumer<CoachPaymentEvent>
    {
        private readonly ILogger<CoachPaymentConsumer> _logger;
        // Inject các service cần thiết

        public CoachPaymentConsumer(ILogger<CoachPaymentConsumer> logger)
        {
            _logger = logger;
        }

        // Xử lý event thanh toán cũ
        public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
        {
            var paymentEvent = context.Message;

            // Chỉ xử lý nếu loại thanh toán liên quan đến Coach service
            if (paymentEvent.PaymentType == "CoachBooking" ||
                paymentEvent.PaymentType == "CoachPackage" ||
                paymentEvent.PaymentType.StartsWith("Coach"))
            {
                _logger.LogInformation("Xử lý thanh toán cho Coach: {TransactionId}", paymentEvent.TransactionId);

                // Thực hiện xử lý thanh toán cho coach
                await ProcessCoachPayment(paymentEvent);
            }
            else
            {
                _logger.LogDebug("Bỏ qua sự kiện thanh toán không phải dành cho Coach service: {PaymentType}",
                    paymentEvent.PaymentType);
            }
        }

        // Xử lý event chuyên biệt cho thanh toán coach
        public async Task Consume(ConsumeContext<CoachPaymentEvent> context)
        {
            var paymentEvent = context.Message;

            _logger.LogInformation("Xử lý thanh toán coach chuyên biệt: {TransactionId}, CoachId: {CoachId}",
                paymentEvent.TransactionId, paymentEvent.CoachId);

            // Xử lý thanh toán chi tiết cho coach
            await ProcessCoachPaymentDetailed(paymentEvent);
        }

        private async Task ProcessCoachPayment(PaymentBaseEvent payment)
        {
            // Xử lý thanh toán cơ bản cho event cũ
        }

        private async Task ProcessCoachPaymentDetailed(CoachPaymentEvent payment)
        {
            // Xử lý thanh toán chi tiết cho coach
            // Đặt lịch hoặc mua gói huấn luyện
        }
    }
}