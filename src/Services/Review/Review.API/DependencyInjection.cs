using Reviews.API.Cache;
using Reviews.API.Clients;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        // Cache
        services.AddSingleton<ISubjectCache, MemorySubjectCache>();

        // HTTP Clients
        services.AddHttpClient<ICoachServiceClient, CoachServiceClient>(client =>
            client.BaseAddress = new Uri(config["CoachService:BaseUrl"]));

        services.AddHttpClient<ICourtServiceClient, CourtServiceClient>(client =>
          client.BaseAddress = new Uri(config["CourtService:BaseUrl"]));

        return services;
    }
}