namespace Identity.Domain.Events
{
    public record ServicePackageSubscribedEvent(
    int PackageId,
    Guid UserId) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}