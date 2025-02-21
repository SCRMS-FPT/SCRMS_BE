namespace Identity.Domain.Events
{
    public record UserCreatedEvent(Guid UserId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}