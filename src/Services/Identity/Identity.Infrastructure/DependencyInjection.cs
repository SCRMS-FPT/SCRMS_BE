using Identity.Application.Data;
using Identity.Domain.Models;
using Identity.Infrastructure.Data;
using Identity.Infrastructure.Data.Interceptors;
using Identity.Infrastructure.Data.Managers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Database");

            // Đăng ký interceptors
            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
            services.AddScoped<UserManager<User>, SoftDeleteUserManager>();
            // Cấu hình DbContext
            services.AddDbContext<IdentityDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString);
        });
            services.AddIdentity<User, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();
            // Đăng ký repository
            services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<IdentityDbContext>());

            return services;
        }
    }
}