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
            if (startTime >= endTime)
                throw new DomainException("Start time must be lower than end time");

            decimal totalPrice = CalculatePrice(startTime, endTime, schedules);

            return new BookingDetail
            {
                Id = BookingDetailId.Of(Guid.NewGuid()),
                BookingId = bookingId,
                CourtId = courtId,
                StartTime = startTime,
                EndTime = endTime,
                TotalPrice = totalPrice,
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };
        }

        public static BookingDetail Create(BookingDetailId id, BookingId bookingId, CourtId courtId,
           TimeSpan startTime, TimeSpan endTime, decimal totalPrice, decimal minDepositPercentage)
        {
            if (startTime >= endTime)
                throw new DomainException("Start time must be lower than end time");

            if (totalPrice < 0)
                throw new DomainException("Total price cannot be negative");

            return new BookingDetail
            {
                Id = id,
                BookingId = bookingId,
                CourtId = courtId,
                StartTime = startTime,
                EndTime = endTime,
                TotalPrice = totalPrice,
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };
        }

        private static decimal CalculatePrice(TimeSpan startTime, TimeSpan endTime, List<CourtSchedule> schedules)
        {
            // Kiểm tra nếu schedules rỗng hoặc null, trả về 0
            if (schedules == null || schedules.Count == 0)
                return 0;
                
            decimal total = 0;
            TimeSpan current = startTime;

            // Sort schedules by StartTime for easier processing
            var sortedSchedules = schedules.OrderBy(s => s.StartTime).ToList();

            while (current < endTime)
            {
                // Find the schedule that covers the current time
                var schedule = sortedSchedules.FirstOrDefault(s => s.StartTime <= current && s.EndTime > current);
                if (schedule == null)
                    throw new DomainException($"No schedule found for time slot starting at {current}");

                // Determine how long we stay in this schedule period
                TimeSpan slotEndTime = schedule.EndTime < endTime ? schedule.EndTime : endTime;
                TimeSpan duration = slotEndTime - current;

                // Calculate price for this period using hourly rate
                decimal priceForPeriod = schedule.PriceSlot * (decimal)duration.TotalHours;
                total += priceForPeriod;

                // Move to the next time boundary
                current = slotEndTime;
            }

            return total;
        }
    }
}
