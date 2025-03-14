using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Outbox
{
    public interface IOutboxService
    {
        Task SaveMessageAsync<T>(T message) where T : class;
        Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken);
    }
}