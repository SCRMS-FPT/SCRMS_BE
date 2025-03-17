using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Messaging.Outbox;
using CourtBooking.API.Extensions;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Infrastructure.Data;
using CourtBooking.Infrastructure.Data.Repositories;
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
        services.AddScoped<ICourtPromotionRepository, CourtPromotionRepository>();
        services.AddOutbox<ApplicationDbContext>();
        // Thêm xác thực và phân quyền
        services.AddJwtAuthentication(configuration);
        services.AddAuthorizationPolicies();

        services.AddExceptionHandler<CustomExceptionHandler>();
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Database")!);

        // Thêm HttpContextAccessor để truy cập thông tin người dùng trong các handler
        services.AddHttpContextAccessor();

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

        // Đảm bảo middleware xác thực được áp dụng
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}