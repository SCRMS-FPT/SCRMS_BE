using System;

namespace CourtBooking.Application.Exceptions
{
    public class BookingNotFoundException : Exception
    {
        public BookingNotFoundException(Guid bookingId)
            : base($"Không tìm thấy đặt sân với ID: {bookingId}")
        {
        }
    }
}