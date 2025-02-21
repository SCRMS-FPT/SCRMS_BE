using BuildingBlocks.Behaviors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            services.AddFeatureManagement();
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            return services;
        }
    }
}