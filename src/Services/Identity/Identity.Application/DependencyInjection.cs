using BuildingBlocks.Behaviors;
using Identity.Application.Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using System.Reflection;
using MassTransit;
using Identity.Application.Consumers;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.Extensions;

namespace Identity.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Đăng ký MediatR và behaviors
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            });
            services.AddScoped<IApplicationDbContext>(provider =>
                (IApplicationDbContext)provider.GetRequiredService<IdentityDbContext>());
            services.AddMassTransit(x =>
            {
                x.AddConsumer<PaymentSucceededConsumer>(cfg =>
                {
                    // Thêm retry policy
                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                });

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["MessageBroker:Host"], h =>
                    {
                        h.Username(configuration["MessageBroker:UserName"]);
                        h.Password(configuration["MessageBroker:Password"]);
                    });

                    // Đăng ký endpoint chỉ nhận các event thanh toán liên quan đến Identity
                    cfg.ReceiveEndpoint("identity-service-payments", e =>
                    {
                        // Cấu hình consumer
                        e.ConfigureConsumer<PaymentSucceededConsumer>(context);

                        // Filter message theo header hoặc message type
                        e.UseFilter(new MessageTypeFilter(
                            typeof(ServicePackagePaymentEvent),
                            typeof(PaymentSucceededEvent) // Nhận cả event cũ để tương thích ngược
                        ));

                        // Cấu hình cho PaymentSucceededEvent cũ - kiểm tra loại thanh toán
                        e.UseFilter(new PaymentSucceededEventFilter("ServicePackage", "AccountUpgrade", "IdentityService"));
                    });
                });
            });
            services.AddFeatureManagement();
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            //services.AddOutbox<IdentityDbContext>();
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
}