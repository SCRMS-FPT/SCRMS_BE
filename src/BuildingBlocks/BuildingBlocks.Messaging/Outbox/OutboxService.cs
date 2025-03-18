using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Messaging.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging.Outbox
{
    public class OutboxService<TDbContext> : IOutboxService
        where TDbContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<OutboxService<TDbContext>> _logger;

        public OutboxService(
            IServiceProvider serviceProvider,
            IPublishEndpoint publishEndpoint,
            ILogger<OutboxService<TDbContext>> logger)
        {
            _serviceProvider = serviceProvider;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task SaveMessageAsync<T>(T message) where T : class
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

            var outboxMessage = new OutboxMessage
            {
                Type = message.GetType().AssemblyQualifiedName!,
                Content = JsonSerializer.Serialize(message)
            };

            await dbContext.Set<OutboxMessage>().AddAsync(outboxMessage);
            await dbContext.SaveChangesAsync();
        }

        public async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

            var messages = await dbContext.Set<OutboxMessage>()
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.CreatedAt) // Thêm ordering
                .Take(20)
                .AsNoTracking() // Tránh tracking không cần thiết
                .ToListAsync(cancellationToken);

            foreach (var message in messages)
            {
                try
                {
                    var messageType = Type.GetType(message.Type);
                    if (messageType == null)
                    {
                        throw new InvalidOperationException($"Type {message.Type} not found");
                    }

                    var publishedMessage = JsonSerializer.Deserialize(message.Content, messageType);
                    if (publishedMessage == null)
                    {
                        throw new InvalidOperationException("Could not deserialize message");
                    }

                    await _publishEndpoint.Publish(publishedMessage, messageType, cancellationToken);

                    message.ProcessedAt = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
                    message.Error = ex.Message;
                    message.ProcessedAt = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}