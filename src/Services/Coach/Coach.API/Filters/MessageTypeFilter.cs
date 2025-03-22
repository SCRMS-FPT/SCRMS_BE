using MassTransit;
using System;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Messaging.Events;

namespace Coach.API.Filters
{
    public class MessageTypeFilter : IFilter<ConsumeContext>
    {
        private readonly Type[] _acceptedTypes;
        private readonly ILogger<MessageTypeFilter> _logger;

        public MessageTypeFilter(ILogger<MessageTypeFilter> logger, params Type[] acceptedTypes)
        {
            _acceptedTypes = acceptedTypes;
            _logger = logger;
        }

        public async Task Send(ConsumeContext context, IPipe<ConsumeContext> next)
        {
            try
            {
                var messageType = context.GetType();
                if (_acceptedTypes.Any(t => messageType.IsAssignableTo(t)))
                {
                    await next.Send(context);
                }
                else
                {
                    if (context.TryGetMessage<PaymentBaseEvent>(out var messageContext))
                    {
                        var messageTypeHeader = context.Headers.Get<string>("payment-type");
                        if (!string.IsNullOrEmpty(messageTypeHeader))
                        {
                            _logger.LogInformation("Processing payment message of type: {MessageType}", messageTypeHeader);
                        }
                    }
                    else
                    {
                        await next.Send(context);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message in filter");
                await next.Send(context);
            }
        }

        public void Probe(ProbeContext context) => context.CreateFilterScope("messageTypeFilter");
    }

    public class PaymentTypeFilter : IFilter<ConsumeContext>
    {
        private readonly string[] _acceptedPaymentTypes;

        public PaymentTypeFilter(params string[] acceptedPaymentTypes)
        {
            _acceptedPaymentTypes = acceptedPaymentTypes;
        }

        public async Task Send(ConsumeContext context, IPipe<ConsumeContext> next)
        {
            // Lấy loại thanh toán từ header đã được thiết lập trước đó
            var paymentType = context.Headers.Get<string>("payment-type");
            if (!string.IsNullOrEmpty(paymentType) &&
                _acceptedPaymentTypes.Any(t => paymentType.Contains(t, StringComparison.OrdinalIgnoreCase)))
            {
                await next.Send(context);
            }
            else
            {
                // Trường hợp không có header hoặc không khớp, vẫn tiếp tục
                // để các filter khác có thể xử lý
                await next.Send(context);
            }
        }

        public void Probe(ProbeContext context) => context.CreateFilterScope("paymentTypeFilter");
    }
}