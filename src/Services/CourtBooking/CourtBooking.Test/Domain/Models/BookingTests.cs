using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Domain.Exceptions;
using CourtBooking.Domain.Events;
using CourtBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CourtBooking.Test.Domain.Models
{
    public class BookingTests
    {
        [Fact]
        public void Create_Should_ReturnValidBooking_WhenParametersAreValid()
        {
            // Arrange
            var id = BookingId.Of(Guid.NewGuid());
            var userId = UserId.Of(Guid.NewGuid());
            var bookingDate = DateTime.Now.Date;
            var note = "Test note";

            // Act
            var booking = Booking.Create(id, userId, bookingDate, note);

            // Assert
            Assert.Equal(id, booking.Id);
            Assert.Equal(userId, booking.UserId);
            Assert.Equal(bookingDate, booking.BookingDate);
            Assert.Equal(note, booking.Note);
            Assert.Equal(BookingStatus.PendingPayment, booking.Status); // Changed from Pending to PendingPayment
            Assert.Equal(0, booking.TotalPrice);
            Assert.Equal(0, booking.RemainingBalance);
            Assert.Equal(0, booking.InitialDeposit);
            Assert.Equal(0, booking.TotalPaid);
            Assert.NotNull(booking.CreatedAt);
        }

        [Fact]
        public void AddBookingDetail_Should_AddDetailAndRecalculateTotals()
        {
            // Arrange
            var booking = CreateValidBooking();
            var courtId = CourtId.Of(Guid.NewGuid());
            var startTime = TimeSpan.FromHours(10);
            var endTime = TimeSpan.FromHours(12);
            var schedules = new List<CourtSchedule>
            {
                CreateCourtSchedule(courtId, TimeSpan.FromHours(9), TimeSpan.FromHours(13), 100m)
            };

            // Act
            booking.AddBookingDetail(courtId, startTime, endTime, schedules);

            // Assert
            Assert.Single(booking.BookingDetails);
            Assert.Equal(courtId, booking.BookingDetails.First().CourtId);
            Assert.Equal(startTime, booking.BookingDetails.First().StartTime);
            Assert.Equal(endTime, booking.BookingDetails.First().EndTime);
            Assert.Equal(2, booking.TotalTime); // 2 giờ
            Assert.Equal(200, booking.TotalPrice); // 2 giờ * 100/giờ = 200
            Assert.Equal(200, booking.RemainingBalance); // Chưa thanh toán
        }

        [Fact]
        public void RemoveBookingDetail_Should_RemoveDetailAndRecalculateTotals()
        {
            // Arrange
            var booking = CreateValidBooking();
            var courtId = CourtId.Of(Guid.NewGuid());
            var startTime = TimeSpan.FromHours(10);
            var endTime = TimeSpan.FromHours(12);
            var schedules = new List<CourtSchedule>
            {
                CreateCourtSchedule(courtId, TimeSpan.FromHours(9), TimeSpan.FromHours(13), 100m)
            };

            booking.AddBookingDetail(courtId, startTime, endTime, schedules);
            var detailId = booking.BookingDetails.First().Id;

            // Act
            booking.RemoveBookingDetail(detailId);

            // Assert
            Assert.Empty(booking.BookingDetails);
            Assert.Equal(0, booking.TotalPrice);
            Assert.Equal(0, booking.RemainingBalance);
        }

        [Fact]
        public void MakeDeposit_Should_UpdateTotalPaidAndStatus_WhenValidDepositAmount()
        {
            // Arrange
            var booking = CreateBookingWithDetail(200m);
            // Đảm bảo số tiền đặt cọc đủ lớn để vượt qua validate
            booking.SetInitialDeposit(100m); // Yêu cầu đặt cọc tối thiểu 100
            var depositAmount = 100m;

            // Act
            booking.MakeDeposit(depositAmount);

            // Assert
            Assert.Equal(depositAmount, booking.InitialDeposit);
            Assert.Equal(depositAmount, booking.TotalPaid);
            Assert.Equal(100m, booking.RemainingBalance); // 200 - 100 = 100
            Assert.Equal(BookingStatus.Deposited, booking.Status); // Changed from Confirmed to Deposited

            // Kiểm tra domain event
            var depositEvent = booking.DomainEvents.Last() as BookingDepositMadeEvent;
            Assert.NotNull(depositEvent);
            Assert.Equal(booking.Id.Value, depositEvent.BookingId);
            Assert.Equal(depositAmount, depositEvent.DepositAmount);
            Assert.Equal(booking.RemainingBalance, depositEvent.RemainingBalance);
        }

        [Fact]
        public void MakeDeposit_Should_UpdateStatusToCompleted_WhenFullAmountPaid()
        {
            // Arrange
            var booking = CreateBookingWithDetail(200m);
            booking.SetInitialDeposit(100m); // Yêu cầu đặt cọc tối thiểu 100
            var depositAmount = 200m; // Đặt cọc toàn bộ

            // Act
            booking.MakeDeposit(depositAmount);

            // Assert
            Assert.Equal(depositAmount, booking.TotalPaid);
            Assert.Equal(0m, booking.RemainingBalance);
            Assert.Equal(BookingStatus.Completed, booking.Status);
        }

        [Fact]
        public void MakeDeposit_Should_ThrowException_WhenNegativeAmount()
        {
            // Arrange
            var booking = CreateBookingWithDetail(200m);
            booking.SetInitialDeposit(100m); // Yêu cầu đặt cọc tối thiểu 100
            var invalidAmount = -50m;

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => booking.MakeDeposit(invalidAmount));
            Assert.Contains("Số tiền đặt cọc tối thiểu", exception.Message);
        }

        [Fact]
        public void MakeDeposit_Should_ThrowException_WhenBelowMinimumDeposit()
        {
            // Arrange
            var booking = CreateBookingWithDetail(200m);
            booking.SetInitialDeposit(100m); // Yêu cầu đặt cọc tối thiểu 100
            var belowMinAmount = 50m; // Dưới mức tối thiểu 100

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => booking.MakeDeposit(belowMinAmount));
            Assert.Contains("Số tiền đặt cọc tối thiểu", exception.Message);
        }

        [Fact]
        public void MakePayment_Should_UpdateTotalPaidAndStatus()
        {
            // Arrange
            var booking = CreateBookingWithDetail(200m);
            booking.SetInitialDeposit(50m); // Yêu cầu đặt cọc tối thiểu 50
            booking.MakeDeposit(50m); // Đã đặt cọc 50
            var paymentAmount = 150m;

            // Act
            booking.MakePayment(paymentAmount);

            // Assert
            Assert.Equal(200m, booking.TotalPaid); // 50 + 150 = 200
            Assert.Equal(0m, booking.RemainingBalance);
            Assert.Equal(BookingStatus.Completed, booking.Status);

            // Kiểm tra domain event
            var paymentEvent = booking.DomainEvents.Last() as BookingPaymentMadeEvent;
            Assert.NotNull(paymentEvent);
            Assert.Equal(booking.Id.Value, paymentEvent.BookingId);
            Assert.Equal(paymentAmount, paymentEvent.PaymentAmount);
            Assert.Equal(booking.RemainingBalance, paymentEvent.RemainingBalance);
        }

        [Fact]
        public void Cancel_Should_UpdateStatusToCancelled()
        {
            // Arrange
            var booking = CreateValidBooking();

            // Act
            booking.Cancel();

            // Assert
            Assert.Equal(BookingStatus.Cancelled, booking.Status);
        }

        [Fact]
        public void Cancel_Should_ThrowDomainException_WhenAlreadyCancelled()
        {
            // Arrange
            var booking = CreateValidBooking();
            booking.Cancel();

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => booking.Cancel());
            Assert.Contains("Booking is already cancelled", exception.Message);
        }

        [Fact]
        public void Confirm_Should_UpdateStatusToConfirmed()
        {
            // Arrange
            var booking = CreateValidBooking();

            // Act
            booking.Confirm();

            // Assert
            Assert.Equal(BookingStatus.Deposited, booking.Status); // Changed from Confirmed to Deposited
        }

        [Fact]
        public void Confirm_Should_ThrowDomainException_WhenNotPending()
        {
            // Arrange
            var booking = CreateValidBooking();
            booking.Confirm(); // Đã chuyển sang Deposited

            // Act & Assert
            var exception = Assert.Throws<DomainException>(() => booking.Confirm());
            Assert.Contains("Only pending bookings can be confirmed", exception.Message);
        }

        [Fact]
        public void SetCancellationReason_Should_UpdateReason()
        {
            // Arrange
            var booking = CreateValidBooking();
            var reason = "Customer changed plans";

            // Act
            booking.SetCancellationReason(reason);

            // Assert
            Assert.Equal(reason, booking.CancellationReason);
        }

        [Fact]
        public void SetCancellationTime_Should_UpdateCancellationTime()
        {
            // Arrange
            var booking = CreateValidBooking();
            var cancellationTime = DateTime.UtcNow;

            // Act
            booking.SetCancellationTime(cancellationTime);

            // Assert
            Assert.Equal(cancellationTime, booking.CancellationTime);
        }

        private Booking CreateValidBooking()
        {
            return Booking.Create(
                BookingId.Of(Guid.NewGuid()),
                UserId.Of(Guid.NewGuid()),
                DateTime.Now.Date,
                "Test booking"
            );
        }

        private Booking CreateBookingWithDetail(decimal price)
        {
            var booking = CreateValidBooking();
            var courtId = CourtId.Of(Guid.NewGuid());
            var startTime = TimeSpan.FromHours(10);
            var endTime = TimeSpan.FromHours(12);
            var schedules = new List<CourtSchedule>
            {
                CreateCourtSchedule(courtId, TimeSpan.FromHours(9), TimeSpan.FromHours(13), price / 2) // price cho 2 giờ
            };

            booking.AddBookingDetail(courtId, startTime, endTime, schedules);
            return booking;
        }

        private CourtSchedule CreateCourtSchedule(
            CourtId courtId,
            TimeSpan startTime,
            TimeSpan endTime,
            decimal priceSlot)
        {
            return CourtSchedule.Create(
                CourtScheduleId.Of(Guid.NewGuid()),
                courtId,
                new DayOfWeekValue(new int[] { 1, 2, 3, 4, 5, 6, 7 }),
                startTime,
                endTime,
                priceSlot
            );
        }
    }
}