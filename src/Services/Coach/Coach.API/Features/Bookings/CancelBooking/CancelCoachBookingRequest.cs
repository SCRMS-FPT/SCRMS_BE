namespace Coach.API.Features.Bookings.CancelBooking
{
    public record CancelCoachBookingRequest(
        string CancellationReason
    );
}
