using MediatR;

namespace Identity.Domain.Abstractions;
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}