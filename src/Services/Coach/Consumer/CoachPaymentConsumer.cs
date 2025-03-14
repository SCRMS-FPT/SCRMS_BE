using BuildingBlocks.Messaging.Events;
using MassTransit;

namespace Coach.Consumer
{
    public class CoachPaymentConsumer : IConsumer<CoachPaymentEvent>
    {
        public async Task Consume(ConsumeContext<CoachPaymentEvent> context)
        {
            var payment = context.Message;

            // Xử lý thanh toán cho coach
            // ...

            // Ghi log
            Console.WriteLine($"Coach Service xử lý thanh toán: {payment.TransactionId}");
        }
    }
}