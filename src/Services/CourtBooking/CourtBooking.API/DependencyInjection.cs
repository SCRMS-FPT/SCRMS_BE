using BuildingBlocks.Exceptions.Handler;
using CourtBooking.Application.Data.Repositories;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace CourtBooking.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCarter();
        services.AddScoped<ICourtRepository, CourtRepository>();
        services.AddScoped<ICourtScheduleRepository, CourtScheduleRepository>();
        services.AddScoped<ISportCenterRepository, SportCenterRepository>();
        services.AddScoped<ISportRepository, SportRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        services.AddExceptionHandler<CustomExceptionHandler>();
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Database")!);

        return services;
    }

    public static WebApplication UseApiServices(this WebApplication app)
    {
        app.MapCarter();

        app.UseExceptionHandler(options => { });
        app.UseHealthChecks("/health",
            new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

        return app;
    }
}