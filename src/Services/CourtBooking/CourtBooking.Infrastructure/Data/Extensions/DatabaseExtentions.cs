

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CourtBooking.Infrastructure.Data.Extensions;

public static class DatabaseExtentions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Database.MigrateAsync().GetAwaiter().GetResult();
        //await SeedAsync(context);
    }

    private static async Task SeedAsync(ApplicationDbContext context)
    {
        await SeedSportAsync(context);
        await SeedCourtAsync(context);
    }

    private static async Task SeedSportAsync(ApplicationDbContext context)
    {
        await context.Sports.AddRangeAsync(InitialData.Sports);
        await context.SaveChangesAsync();
    }
    private static async Task SeedCourtAsync(ApplicationDbContext context)
    {
        await context.Courts.AddRangeAsync(InitialData.Courts);
        await context.SaveChangesAsync();
    }
}
