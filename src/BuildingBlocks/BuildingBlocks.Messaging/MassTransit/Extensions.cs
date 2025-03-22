using BuildingBlocks.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace BuildingBlocks.Messaging.MassTransit;

public static class Extensions
{
    public static IServiceCollection AddMessageBroker(this IServiceCollection services, IConfiguration configuration, Assembly assembly, Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        services.AddMassTransit(config =>
        {
            // Đăng ký consumers
            if (configureConsumers != null)
            {
                configureConsumers(config);
            }
            else
            {
                // Đăng ký toàn bộ consumers trong assembly
                config.AddConsumers(assembly);
            }

            // Cấu hình kết nối message broker
            config.UsingRabbitMq((context, configurator) =>
            {
                // Đọc cấu hình từ appsettings.json
                var host = configuration["MessageBroker:Host"] ?? "rabbitmq://localhost";
                var username = configuration["MessageBroker:UserName"] ?? "guest";
                var password = configuration["MessageBroker:Password"] ?? "guest";

                configurator.Host(new Uri(host), h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // Thêm filter để xử lý messages
                services.AddSingleton<IFilter<PublishContext<PaymentBaseEvent>>, PaymentTypeHeaderFilter>();

                configurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    // Filter để thêm header cho các message
    public class PaymentTypeHeaderFilter : IFilter<PublishContext<PaymentBaseEvent>>
    {
        public void Probe(ProbeContext context) => context.CreateFilterScope("paymentTypeHeader");

        public Task Send(PublishContext<PaymentBaseEvent> context, IPipe<PublishContext<PaymentBaseEvent>> next)
        {
            // Thêm header để định danh loại message
            context.Headers.Set("payment-type", context.Message.GetType().Name);

            // Continue with the message pipeline
            return next.Send(context);
        }
    }
}