using BuildingBlocks.Exceptions.Handler;
using HealthChecks.UI.Client;
using Identity.Application.Data.Repositories;
using Identity.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Identity.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApiServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IServicePackageRepository, ServicePackageRepository>();
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

            services.AddCarter();
            services.AddExceptionHandler<CustomExceptionHandler>();
            services.AddHealthChecks()
                .AddNpgSql(configuration.GetConnectionString("Database")!);

            return services;
        }

        public static WebApplication UseApiServices(this WebApplication app)
        {
            // Map Carter endpoints
            app.MapCarter();

            app.UseExceptionHandler(options => { });

            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            return app;
        }
    }
}