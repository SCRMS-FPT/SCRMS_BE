using BuildingBlocks.Exceptions.Handler;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Identity.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApiServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Thêm Carter cho API endpoints
            services.AddCarter();

            // Xử lý exception
            services.AddExceptionHandler<CustomExceptionHandler>();

            // Health checks
            services.AddHealthChecks()
                .AddNpgSql(configuration.GetConnectionString("Postgres")!);

            return services;
        }

        public static WebApplication UseApiServices(this WebApplication app)
        {
            // Map Carter endpoints
            app.MapCarter();

            // Cấu hình exception handler
            app.UseExceptionHandler(options => { });

            // Cấu hình health checks
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            return app;
        }
    }
}
