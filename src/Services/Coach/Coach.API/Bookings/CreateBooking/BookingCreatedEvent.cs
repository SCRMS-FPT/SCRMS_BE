namespace Coach.API.Bookings.CreateBooking
{
    public record BookingCreatedEvent(Guid BookingId, Guid UserId, Guid CoachId) : INotification;
}