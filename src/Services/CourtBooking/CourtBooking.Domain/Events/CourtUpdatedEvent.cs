using CourtBooking.Domain.Abstractions;
using CourtBooking.Domain.Models;

namespace CourtBooking.Domain.Events;
public record CourtUpdatedEvent(Guid CourtId, string Name, decimal PricePerHour) : IDomainEvent;


