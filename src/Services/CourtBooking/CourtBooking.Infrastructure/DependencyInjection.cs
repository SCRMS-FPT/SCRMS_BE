using CourtBooking.Infrastructure.Data.Interceptor;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CourtBooking.Application.Data;
using CourtBooking.Infrastructure.Services;

namespace CourtBooking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices
        (this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.Configure<ImageKitOptions>(configuration.GetSection("ImageKit"));
        services.AddHttpClient<IFileStorageService, ImageKitStorageService>();
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

        return services;
    }
}