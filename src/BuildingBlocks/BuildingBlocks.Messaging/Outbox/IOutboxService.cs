using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Messaging.Events;

namespace BuildingBlocks.Messaging.Outbox
{
    public interface IOutboxService
    {
        Task SaveMessageAsync<T>(T message) where T : class;

        Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : class;

        Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken);
    }
}