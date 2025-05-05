using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Messaging.Outbox;
using Coach.API.Data;

namespace Coach.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOutboxServices(this IServiceCollection services)
        {
            services.AddOutbox<CoachDbContext>();

            return services;
        }
    }
}