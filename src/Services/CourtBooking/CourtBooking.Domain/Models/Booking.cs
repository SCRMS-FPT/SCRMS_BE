using System;
using System.Collections.Generic;
using System.Linq;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Domain.Exceptions;

namespace CourtBooking.Domain.Models
{
    public class Booking : Aggregate<BookingId>
    {
        public UserId UserId { get; private set; }
        public CourtId CourtId { get; private set; }
        public DateTime BookingDate { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public BookingStatus Status { get; private set; }
        public PromotionId? PromotionId { get; private set; }
        private List<BookingPrice> _bookingPrices = new();
        public IReadOnlyCollection<BookingPrice> BookingPrices => _bookingPrices.AsReadOnly();

        private Booking() { } // For ORM

        public static Booking Create(UserId userId, CourtId courtId, DateTime bookingDate, TimeSpan startTime, TimeSpan endTime, PromotionId? promotionId = null)
        {
            if (endTime <= startTime)
                throw new DomainException("End time must be after start time.");

            return new Booking
            {
                Id = BookingId.Of(Guid.NewGuid()),
                UserId = userId,
                CourtId = courtId,
                BookingDate = bookingDate,
                StartTime = startTime,
                EndTime = endTime,
                Status = BookingStatus.Pending,
                PromotionId = promotionId
            };
        }

        public void AddPriceSegment(TimeSpan start, TimeSpan end, decimal price)
        {
            if (end <= start)
                throw new DomainException("End time must be after start time.");
            if (price < 0)
                throw new DomainException("Price must be non-negative.");

            _bookingPrices.Add(BookingPrice.Create(Id, start, end, price));
        }

        public void Confirm()
        {
            if (Status != BookingStatus.Pending)
                throw new DomainException("Only pending bookings can be confirmed.");
            Status = BookingStatus.Confirmed;
        }

        public void Cancel()
        {
            if (Status == BookingStatus.Cancelled)
                throw new DomainException("Booking is already cancelled.");
            Status = BookingStatus.Cancelled;
        }

        public decimal GetTotalPrice()
        {
            return _bookingPrices.Sum(bp => bp.Price);
        }
    }
}
