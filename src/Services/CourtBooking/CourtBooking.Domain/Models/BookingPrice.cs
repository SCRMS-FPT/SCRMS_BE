using CourtBooking.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Domain.Models
{
    public class BookingPrice : Entity<BookingPriceId>
    {
        public BookingId BookingId { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public decimal Price { get; private set; }

        protected BookingPrice() { } // Required for EF Core

        public static BookingPrice Create(BookingId bookingId, TimeSpan startTime, TimeSpan endTime, decimal price)
        {
            if (endTime <= startTime)
                throw new DomainException("Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");
            if (price < 0)
                throw new DomainException("Giá tiền không hợp lệ.");

            return new BookingPrice
            {
                Id = BookingPriceId.Of(Guid.NewGuid()),
                BookingId = bookingId,
                StartTime = startTime,
                EndTime = endTime,
                Price = price
            };
        }
    }
}
