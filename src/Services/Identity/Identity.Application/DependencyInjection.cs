using BuildingBlocks.Behaviors;
using BuildingBlocks.Messaging.Events;
using Identity.Application.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using System.Reflection;

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
               // Log when consumer is registered
               var serviceProvider = services.BuildServiceProvider();
               var logger = serviceProvider.GetRequiredService<ILogger<PaymentSucceededConsumer>>();

               // Register consumers
               x.AddConsumer<PaymentSucceededConsumer>(cfg =>
               {
                   logger.LogInformation("Registering PaymentSucceededConsumer");
                   cfg.UseMessageRetry(r => r.Interval(3, 1000));
               });

               // Log the event types we're set up to consume
               logger.LogInformation("Identity service set up to consume: PaymentSucceededEvent, ServicePackagePaymentEvent");
               logger.LogInformation("ServicePackagePaymentEvent type: {Type}", typeof(ServicePackagePaymentEvent).AssemblyQualifiedName);

               // Configure RabbitMQ
               x.UsingRabbitMq((context, cfg) =>
               {
                   // Log connection information
                   var host = configuration["MessageBroker:Host"];
                   logger.LogInformation("Configuring RabbitMQ connection to host: {Host}", host);

                   cfg.Host(host, h =>
                   {
                       h.Username(configuration["MessageBroker:UserName"]);
                       h.Password(configuration["MessageBroker:Password"]);
                   });

                   // CRITICAL: Set up a specific receive endpoint for Identity service
                   cfg.ReceiveEndpoint("identity-service-queue", e =>
                   {
                       logger.LogInformation("Configuring receive endpoint: identity-service-queue");

                       // Configure the consumer
                       e.ConfigureConsumer<PaymentSucceededConsumer>(context);
                   });

                   // This adds diagnostics to track message activity
                   cfg.UseInMemoryOutbox();
                   cfg.ConfigureEndpoints(context);

                   logger.LogInformation("MassTransit configuration completed");
               });
           });
            services.AddFeatureManagement();
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<GoogleSettings>(configuration.GetSection("Google"));
            services.Configure<EndpointSettings>(configuration.GetSection("Endpoints"));

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