using CourtBooking.Domain.Abstractions;

namespace CourtBooking.Domain.Events;
public record CourtDeletedEvent(Guid CourtId) : IDomainEvent;

