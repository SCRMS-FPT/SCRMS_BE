using CourtBooking.Domain.Abstractions;

namespace CourtBooking.Domain.Events;
public record BookingPaymentMadeEvent(Guid BookingId, decimal PaymentAmount, decimal RemainingBalance) : IDomainEvent;