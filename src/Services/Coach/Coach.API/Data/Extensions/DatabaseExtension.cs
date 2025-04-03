using Microsoft.EntityFrameworkCore;

namespace Coach.API.Data.Extensions
{
    public static class DatabaseExtentions
    {
        public static async Task InitialiseDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<CoachDbContext>();

            var isDatabaseCreated = await context.Database.CanConnectAsync();
            if (!isDatabaseCreated)
            {
                await context.Database.MigrateAsync();
            }
        }
    }
}
