using System;
using System.Collections.Generic;
using System.Linq;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Domain.Exceptions;
using CourtBooking.Domain.Events;

namespace CourtBooking.Domain.Models
{
    public class Booking : Aggregate<BookingId>
    {
        public UserId UserId { get; private set; }
        public DateTime BookingDate { get; private set; }
        public BookingStatus Status { get; private set; }
        public decimal TotalTime { get; private set; }
        public decimal TotalPrice { get; private set; }
        public decimal RemainingBalance { get; private set; }
        public decimal InitialDeposit { get; private set; }
        public decimal TotalPaid { get; private set; }
        public string? Note { get; private set; }

        private List<BookingDetail> _bookingDetails = new();
        public IReadOnlyCollection<BookingDetail> BookingDetails => _bookingDetails.AsReadOnly();

        private Booking()
        { } // For ORM

        public static Booking Create(BookingId id, UserId userId, DateTime bookingDate, string note = null)
        {
            return new Booking
            {
                Id = id,
                UserId = userId,
                BookingDate = bookingDate,
                Status = BookingStatus.Pending,
                Note = note,
                TotalPrice = 0,
                RemainingBalance = 0,
                InitialDeposit = 0,
                TotalPaid = 0,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void AddBookingDetail(CourtId courtId, TimeSpan startTime, TimeSpan endTime, List<CourtSchedule> schedules, decimal minDepositPercentage = 100)
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

        public void MakeDeposit(decimal depositAmount)
        {
            if (depositAmount <= 0)
                throw new DomainException("Số tiền đặt cọc phải lớn hơn 0");

            if (depositAmount > RemainingBalance)
                throw new DomainException("Số tiền đặt cọc không thể lớn hơn số dư còn lại");

            TotalPaid += depositAmount;
            RemainingBalance = TotalPrice - TotalPaid;

            if (Status == BookingStatus.Pending)
            {
                InitialDeposit = depositAmount;
                Status = BookingStatus.Confirmed;
            }

            if (RemainingBalance == 0)
            {
                Status = BookingStatus.Deposited;
            }

            AddDomainEvent(new BookingDepositMadeEvent(Id.Value, depositAmount, RemainingBalance));
        }

        public void MakePayment(decimal paymentAmount)
        {
            if (paymentAmount <= 0)
                throw new DomainException("Số tiền thanh toán phải lớn hơn 0");

            if (paymentAmount > RemainingBalance)
                throw new DomainException("Số tiền thanh toán không thể lớn hơn số dư còn lại");

            TotalPaid += paymentAmount;
            RemainingBalance = TotalPrice - TotalPaid;

            if (RemainingBalance == 0)
            {
                Status = BookingStatus.Deposited;
            }

            AddDomainEvent(new BookingPaymentMadeEvent(Id.Value, paymentAmount, RemainingBalance));
        }

        private void RecalculateTotals()
        {
            TotalTime = _bookingDetails.Sum(d => (decimal)(d.EndTime - d.StartTime).TotalHours);
            TotalPrice = _bookingDetails.Sum(d => d.TotalPrice);
            RemainingBalance = TotalPrice - TotalPaid;
        }
    }
}