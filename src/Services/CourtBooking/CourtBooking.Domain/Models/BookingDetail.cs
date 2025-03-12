using CourtBooking.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CourtBooking.Domain.Models
{
    public class BookingDetail : Entity<BookingDetailId>
    {
        public BookingId BookingId { get; private set; }
        public CourtId CourtId { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public decimal TotalPrice { get; private set; }

        protected BookingDetail() { } // For EF Core

        public static BookingDetail Create(BookingId bookingId, CourtId courtId, TimeSpan startTime, TimeSpan endTime, List<CourtSchedule> schedules)
        {
            if (startTime <= endTime)
                throw new DomainException("Start time must be lower than end time");

            decimal totalPrice = CalculatePrice(startTime, endTime, schedules);

            return new BookingDetail
            {
                Id = BookingDetailId.Of(Guid.NewGuid()),
                BookingId = bookingId,
                CourtId = courtId,
                StartTime = startTime,
                EndTime = endTime,
                TotalPrice = totalPrice
            };
        }

        private static decimal CalculatePrice(TimeSpan startTime, TimeSpan endTime, List<CourtSchedule> schedules)
        {
            decimal total = 0;
            TimeSpan current = startTime;

            while (current < endTime)
            {
                var schedule = schedules.FirstOrDefault(s => s.StartTime <= current && s.EndTime > current);
                if (schedule == null)
                    throw new DomainException("Not found this slot");

                TimeSpan nextBoundary = schedules
                    .Where(s => s.StartTime > current)
                    .OrderBy(s => s.StartTime)
                    .Select(s => s.StartTime)
                    .FirstOrDefault();

                if (nextBoundary == default || nextBoundary > endTime)
                    nextBoundary = endTime;

                decimal pricePerHour = schedule.PriceSlot;
                total += pricePerHour * (decimal)(nextBoundary - current).TotalHours;

                current = nextBoundary;
            }

            return total;
        }
    }
}
