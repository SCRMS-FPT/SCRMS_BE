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
        public DateTime BookingDate { get; private set; }
        public BookingStatus Status { get; private set; }
        public decimal TotalTime { get; private set; }
        public decimal TotalPrice { get; private set; }
        public string? Note { get; private set; }

        private List<BookingDetail> _bookingDetails = new();
        public IReadOnlyCollection<BookingDetail> BookingDetails => _bookingDetails.AsReadOnly();

        private Booking() { } // For ORM

        public static Booking Create(UserId userId, DateTime bookingDate, string? note = null)
        {
            return new Booking
            {
                Id = BookingId.Of(Guid.NewGuid()),
                UserId = userId,
                BookingDate = bookingDate,
                Status = BookingStatus.Pending,
                Note = note,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void AddBookingDetail(CourtId courtId, TimeSpan startTime, TimeSpan endTime, List<CourtSchedule> schedules)
        {
            var bookingDetail = BookingDetail.Create(Id, courtId, startTime, endTime, schedules);
            _bookingDetails.Add(bookingDetail);
            RecalculateTotals();
        }

        public void RemoveBookingDetail(BookingDetailId detailId)
        {
            var detail = _bookingDetails.FirstOrDefault(d => d.Id == detailId);
            if (detail != null)
            {
                _bookingDetails.Remove(detail);
                RecalculateTotals();
            }
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

        private void RecalculateTotals()
        {
            TotalTime = _bookingDetails.Sum(d => (decimal)(d.EndTime - d.StartTime).TotalHours);
            TotalPrice = _bookingDetails.Sum(d => d.TotalPrice);
        }
    }
}
