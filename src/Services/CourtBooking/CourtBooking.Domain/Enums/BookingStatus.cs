namespace CourtBooking.Domain.Enums
{
    public enum BookingStatus
    {
        Pending = 0,
        Confirmed = 1,
        Cancelled = 2,
        Completed = 3,

        Deposited = 4,
        PendingPayment = 5
    }
}