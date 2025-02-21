using CourtBooking.Domain.Abstractions;

namespace CourtBooking.Domain.Events;
public record CourtCreatedEvent(Guid CourtId, string Name, Guid OwnerId) : IDomainEvent;

