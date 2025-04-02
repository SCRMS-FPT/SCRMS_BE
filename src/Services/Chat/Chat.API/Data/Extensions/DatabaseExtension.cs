using Microsoft.EntityFrameworkCore;

namespace Chat.API.Data.Extensions
{
    public static class DatabaseExtentions
    {
        public static async Task InitialiseDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ChatDbContext>();

            context.Database.MigrateAsync().GetAwaiter().GetResult();
        }
    }
}
