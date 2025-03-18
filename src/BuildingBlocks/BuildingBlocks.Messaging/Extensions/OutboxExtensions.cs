using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging.Extensions
{
    public static class OutboxExtensions
    {
        public static IServiceCollection AddOutbox<TDbContext>(this IServiceCollection services)
            where TDbContext : DbContext
        {
            services.AddScoped<IOutboxService, OutboxService<TDbContext>>();
            services.AddHostedService<OutboxProcessor>();

            return services;
        }

        public static ModelBuilder ConfigureOutbox(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.ProcessedAt);
                entity.Property(e => e.Error);

                entity.HasIndex(e => e.ProcessedAt);
            });

            return modelBuilder;
        }
    }
}