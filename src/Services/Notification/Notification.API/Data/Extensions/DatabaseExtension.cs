using Microsoft.EntityFrameworkCore;

namespace Notification.API.Data.Extensions
{
    public static class DatabaseExtentions
    {
        public static async Task InitialiseDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

            context.Database.MigrateAsync().GetAwaiter().GetResult();
        }
    }
}
