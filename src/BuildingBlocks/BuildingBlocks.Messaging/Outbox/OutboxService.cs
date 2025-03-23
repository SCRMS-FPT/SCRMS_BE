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

        private async Task SaveMessageAsync<T>(T message, string type, CancellationToken cancellationToken = default) where T : class
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

            var outboxMessage = new OutboxMessage
            {
                Type = type,
                Content = JsonSerializer.Serialize(message)
            };

            await dbContext.Set<OutboxMessage>().AddAsync(outboxMessage, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

            // IMPORTANT: Remove AsNoTracking() to allow entity updates
            var messages = await dbContext.Set<OutboxMessage>()
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.CreatedAt)
                .Take(20)
                .ToListAsync(cancellationToken);  // Removed AsNoTracking()

            _logger.LogInformation("Found {MessageCount} unprocessed messages", messages.Count);

            foreach (var message in messages)
            {
                try
                {
                    _logger.LogInformation("Processing message {MessageId} of type {MessageType}",
                        message.Id, message.Type);

                    var messageType = Type.GetType(message.Type);
                    if (messageType == null)
                    {
                        _logger.LogError("Type {MessageType} not found", message.Type);
                        message.Error = $"Type {message.Type} not found";
                        message.ProcessedAt = DateTime.UtcNow;  // Mark as processed with error
                        await dbContext.SaveChangesAsync(cancellationToken);
                        continue;
                    }

                    var publishedMessage = JsonSerializer.Deserialize(message.Content, messageType);
                    if (publishedMessage == null)
                    {
                        _logger.LogError("Failed to deserialize message {MessageId}", message.Id);
                        message.Error = "Failed to deserialize message";
                        message.ProcessedAt = DateTime.UtcNow;  // Mark as processed with error
                        await dbContext.SaveChangesAsync(cancellationToken);
                        continue;
                    }

                    _logger.LogInformation("Publishing message {MessageId} to message broker", message.Id);
                    await _publishEndpoint.Publish(publishedMessage, messageType, cancellationToken);

                    // CRITICAL FIX: Mark as processed after successful publishing
                    message.ProcessedAt = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Message {MessageId} processed successfully", message.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message {MessageId}", message.Id);
                    message.Error = ex.ToString();
                    message.ProcessedAt = DateTime.UtcNow;  // Mark as processed with error
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
            }
        }
        public Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : class
        {
            throw new NotImplementedException();
        }
    }
}