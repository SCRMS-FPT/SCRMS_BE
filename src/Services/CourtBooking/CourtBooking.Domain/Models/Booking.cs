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
        public string? CancellationReason { get; private set; }
        public DateTime? CancellationTime { get; private set; }

        private List<BookingDetail> _bookingDetails = new();
        public IReadOnlyCollection<BookingDetail> BookingDetails => _bookingDetails.AsReadOnly();

        private Booking()
        { } // For ORM

        public static Booking Create(BookingId id, UserId userId, DateTime bookingDate, string? note = null)
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
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };
        }

        public void AddBookingDetail(CourtId courtId, TimeSpan startTime, TimeSpan endTime, List<CourtSchedule> schedules, decimal minDepositPercentage = 100)
        {
            var bookingDetail = BookingDetail.Create(Id, courtId, startTime, endTime, schedules);
            _bookingDetails.Add(bookingDetail);
            InitialDeposit += bookingDetail.TotalPrice * (minDepositPercentage / 100m);
            RecalculateTotals();
        }
        public BookingDetail AddBookingDetailWithPromotion(CourtId courtId, TimeSpan startTime, TimeSpan endTime,
            List<CourtSchedule> schedules, decimal minDepositPercentage, string discountType, decimal discountValue)
        {
            // Find appropriate schedule
            var schedule = schedules.FirstOrDefault(s =>
                s.StartTime <= startTime && s.EndTime >= endTime);

            if (schedule == null)
            {
                throw new InvalidOperationException($"No valid schedule found for time slot {startTime} to {endTime}");
            }

            // Calculate original price
            decimal originalPrice = schedule.PriceSlot;

            // Calculate price after promotion
            decimal finalPrice = originalPrice;
            if (discountType.ToLower() == "percentage")
            {
                // Apply percentage discount
                finalPrice = originalPrice * (1 - (discountValue / 100));
            }
            else if (discountType.ToLower() == "fixed")
            {
                // Apply fixed amount discount
                finalPrice = Math.Max(0, originalPrice - discountValue);
            }

            // Create booking detail with discounted price
            var bookingDetail = BookingDetail.Create(
                BookingDetailId.Of(Guid.NewGuid()),
                Id,
                courtId,
                startTime,
                endTime,
                finalPrice,
                minDepositPercentage
            );

            _bookingDetails.Add(bookingDetail);
            InitialDeposit += bookingDetail.TotalPrice * (minDepositPercentage / 100m);
            RecalculateTotals();
            return bookingDetail;
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
        // Add this method to the Booking class in d:\SEP490_G37\m\SCRMS_BE\src\Services\CourtBooking\CourtBooking.Domain\Models\Booking.cs

        public void UpdateNote(string note)
        {
            Note = note;
            SetLastModified(DateTime.UtcNow);
        }
        public void MarkAsPendingPayment()
        {
            Status = BookingStatus.PendingPayment;
        }

        public void Cancel()
        {
            if (Status == BookingStatus.Cancelled)
                throw new DomainException("Booking is already cancelled.");
            Status = BookingStatus.Cancelled;
        }

        public void SetInitialDeposit(decimal depositAmount)
        {
            InitialDeposit = depositAmount;
        }

        public void MakeDeposit(decimal depositAmount)
        {
            decimal minRequired = InitialDeposit - TotalPaid;

            if (depositAmount < minRequired)
                throw new DomainException($"Số tiền đặt cọc tối thiểu: {minRequired}");

            TotalPaid += depositAmount;
            RemainingBalance = TotalPrice - TotalPaid;

            if (Status == BookingStatus.Pending && TotalPaid >= InitialDeposit)
            {
                Status = BookingStatus.Confirmed;
            }
            if (TotalPaid > InitialDeposit)
            {
                Status = BookingStatus.Deposited;
            }

            if (RemainingBalance == 0)
            {
                Status = BookingStatus.Completed;
            }

            AddDomainEvent(new BookingDepositMadeEvent(Id.Value, depositAmount, RemainingBalance));
        }

        public void MakePayment(decimal paymentAmount)
        {
            if (paymentAmount <= 0)
                throw new DomainException("Số tiền thanh toán phải lớn hơn 0");


            TotalPaid += paymentAmount;
            RemainingBalance = TotalPrice - TotalPaid;

            if (RemainingBalance == 0)
            {
                Status = BookingStatus.Completed;
            }

            AddDomainEvent(new BookingPaymentMadeEvent(Id.Value, paymentAmount, RemainingBalance));
        }

        private void RecalculateTotals()
        {
            TotalTime = _bookingDetails.Sum(d => (decimal)(d.EndTime - d.StartTime).TotalHours);
            TotalPrice = _bookingDetails.Sum(d => d.TotalPrice);

            RemainingBalance = TotalPrice - TotalPaid;
        }

        public void UpdateStatus(BookingStatus newStatus)
        {
            Status = newStatus;
        }

        public void SetCancellationReason(string reason)
        {
            CancellationReason = reason;
        }

        public void SetCancellationTime(DateTime cancelledAt)
        {
            CancellationTime = cancelledAt;
        }
    }
}