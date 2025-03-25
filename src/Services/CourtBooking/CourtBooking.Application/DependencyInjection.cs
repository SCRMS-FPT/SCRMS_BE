using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BuildingBlocks.Messaging.Events;
using CourtBooking.Application.Consumers;
using Microsoft.Extensions.Logging;

namespace CourtBooking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices
        (this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        services.AddMassTransit(configure =>
        {
            // Đăng ký consumer PaymentSucceededConsumer
            configure.AddConsumer<PaymentSucceededConsumer>();
            configure.AddConsumer<BookCourtSucceededConsumer>();
            configure.AddConsumer<PaymentFailedConsumer>();
            services.AddMassTransit(x =>
            {
                x.AddConsumer<PaymentSucceededConsumer>(cfg =>
                {
                    // Thêm retry policy
                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                });
                x.AddConsumer<BookCourtSucceededConsumer>(cfg =>
                {
                    cfg.UseMessageRetry(r => r.Interval(3, 1000));
                });
                x.AddConsumer<PaymentFailedConsumer>(cfg =>
                {
                    cfg.UseMessageRetry(r => r.Interval(3, 1000));
                });
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["MessageBroker:Host"], h =>
                    {
                        h.Username(configuration["MessageBroker:UserName"]);
                        h.Password(configuration["MessageBroker:Password"]);
                    });

                    // Đăng ký endpoint chỉ nhận các event thanh toán liên quan đến Identity
                    cfg.ReceiveEndpoint("court-booking-payment", e =>
                    {
                        // Cấu hình consumer
                        e.ConfigureConsumer<BookCourtSucceededConsumer>(context);
                        e.ConfigureConsumer<PaymentSucceededConsumer>(context);
                        e.ConfigureConsumer<PaymentFailedConsumer>(context);
                    });
                    cfg.UseInMemoryOutbox();
                    cfg.ConfigureEndpoints(context);
                });
            });
        });

        return services;
    }

    // MessageTypeFilter để lọc theo loại message
    public class MessageTypeFilter : IFilter<ConsumeContext>
    {
        private readonly Type[] _acceptedTypes;

        public MessageTypeFilter(params Type[] acceptedTypes)
        {
            _acceptedTypes = acceptedTypes;
        }

        public async Task Send(ConsumeContext context, IPipe<ConsumeContext> next)
        {
            if (_acceptedTypes.Any(t => context.GetType().IsAssignableTo(t)))
            {
                await next.Send(context);
            }
        }

        public void Probe(ProbeContext context) => context.CreateFilterScope("messageTypeFilter");
    }

    // Filter kiểm tra loại thanh toán trong PaymentSucceededEvent
    public class PaymentSucceededEventFilter : IFilter<ConsumeContext<PaymentSucceededEvent>>
    {
        private readonly string[] _acceptedPaymentTypes;

        public PaymentSucceededEventFilter(params string[] acceptedPaymentTypes)
        {
            _acceptedPaymentTypes = acceptedPaymentTypes;
        }

        public async Task Send(ConsumeContext<PaymentSucceededEvent> context, IPipe<ConsumeContext<PaymentSucceededEvent>> next)
        {
            if (_acceptedPaymentTypes.Any(t => context.Message.PaymentType.Contains(t, StringComparison.OrdinalIgnoreCase)))
            {
                await next.Send(context);
            }
        }

        public void Probe(ProbeContext context) => context.CreateFilterScope("paymentTypeFilter");
    }
}