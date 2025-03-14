using BuildingBlocks.Messaging.Events;
using MassTransit;

namespace Identity.Consumer
{
    public class ServicePackagePaymentConsumer : IConsumer<ServicePackagePaymentEvent>
    {
        // Xử lý thanh toán gói dịch vụ
        public async Task Consume(ConsumeContext<ServicePackagePaymentEvent> context)
        {
            var payment = context.Message;

            // Xử lý nâng cấp tài khoản
            // ...

            // Ghi log
            Console.WriteLine($"Identity Service xử lý thanh toán gói dịch vụ: {payment.TransactionId}");
        }
    }
}