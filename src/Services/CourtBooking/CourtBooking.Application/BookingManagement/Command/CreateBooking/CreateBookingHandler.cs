using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourtBooking.Domain.Exceptions;
using CourtBooking.Application.Data.Repositories;
using BuildingBlocks.Messaging.Outbox;
using CourtBooking.Domain.Enums;

namespace CourtBooking.Application.BookingManagement.Command.CreateBooking
{
    public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, CreateBookingResult>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ICourtScheduleRepository _courtScheduleRepository;
        private readonly ICourtRepository _courtRepository;
        private readonly ICourtPromotionRepository _courtPromotionRepository;
        private readonly IOutboxService _outboxService;

        public CreateBookingHandler(
            IBookingRepository bookingRepository,
            ICourtScheduleRepository courtScheduleRepository,
            ICourtRepository courtRepository,
            ICourtPromotionRepository courtPromotionRepository,
            IOutboxService outboxService)
        {
            _bookingRepository = bookingRepository;
            _courtScheduleRepository = courtScheduleRepository;
            _courtRepository = courtRepository;
            _courtPromotionRepository = courtPromotionRepository; // Add this
            _outboxService = outboxService;
        }

        public async Task<CreateBookingResult> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            var userId = UserId.Of(request.UserId);

            // Tạo booking
            var booking = Booking.Create(
                id: BookingId.Of(Guid.NewGuid()),
                userId: userId,
                bookingDate: request.Booking.BookingDate,
                note: request.Booking.Note
            );
            // Thêm chi tiết booking
            foreach (var detail in request.Booking.BookingDetails)
            {
                var courtId = CourtId.Of(detail.CourtId);
                var court = await _courtRepository.GetCourtByIdAsync(courtId, cancellationToken);
                if (court == null)
                {
                    throw new ApplicationException($"Court {detail.CourtId} not found");
                }

                var bookingDayOfWeekInt = request.Booking.BookingDate.DayOfWeek == DayOfWeek.Sunday
                    ? 7
                    : (int)request.Booking.BookingDate.DayOfWeek;
                var allCourtSchedules = await _courtScheduleRepository.GetSchedulesByCourtIdAsync(courtId, cancellationToken);
                var schedules = allCourtSchedules
                    .Where(s => s.DayOfWeek.Days.Contains(bookingDayOfWeekInt))
                    .ToList();
                if (!schedules.Any())
                {
                    throw new ApplicationException($"No schedules found for court {courtId.Value} on day {bookingDayOfWeekInt}");
                }

                // Kiểm tra xem slot đã được đặt chưa (trừ booking đã cancel)
                var existingBookings = await _bookingRepository.GetBookingsInDateRangeForCourtAsync(
                    detail.CourtId,
                    request.Booking.BookingDate,
                    request.Booking.BookingDate,
                    cancellationToken);

                var conflictingBooking = existingBookings
                    .Where(b => b.Status != Domain.Enums.BookingStatus.Cancelled && b.Status != Domain.Enums.BookingStatus.PaymentFail)
                    .SelectMany(b => b.BookingDetails)
                    .Where(bd =>
                        bd.CourtId.Value == detail.CourtId &&
                        ((bd.StartTime <= detail.StartTime && bd.EndTime > detail.StartTime) ||
                         (bd.StartTime < detail.EndTime && bd.EndTime >= detail.EndTime) ||
                         (bd.StartTime >= detail.StartTime && bd.EndTime <= detail.EndTime)))
                    .FirstOrDefault();

                if (conflictingBooking != null)
                {
                    throw new ApplicationException($"Khung giờ từ {detail.StartTime} đến {detail.EndTime} đã được đặt");
                }

                // Kiểm tra và lấy khuyến mãi đang áp dụng cho sân
                var validPromotions = await _courtPromotionRepository.GetValidPromotionsForCourtAsync(
                    courtId,
                    request.Booking.BookingDate,
                    request.Booking.BookingDate,
                    cancellationToken);

                // Lấy khuyến mãi có lợi nhất cho khách hàng
                var bestPromotion = validPromotions
                    .OrderByDescending(p => p.DiscountType.ToLower() == "percentage" ?
                        p.DiscountValue :
                        p.DiscountValue / 100) // Ưu tiên % giảm giá cao nhất
                    .FirstOrDefault();

                // Add booking detail với promotion (nếu có)
                if (bestPromotion != null)
                {
                    // Tính giá sau khi áp dụng khuyến mãi
                    // Lưu ý: Cần điều chỉnh phương thức AddBookingDetail để xử lý khuyến mãi
                    booking.AddBookingDetailWithPromotion(
                        courtId,
                        detail.StartTime,
                        detail.EndTime,
                        schedules,
                        court.MinDepositPercentage,
                        bestPromotion.DiscountType,
                        bestPromotion.DiscountValue);
                }
                else
                {
                    // Nếu không có khuyến mãi, sử dụng phương thức gốc
                    booking.AddBookingDetail(courtId, detail.StartTime, detail.EndTime, schedules, court.MinDepositPercentage);
                }
            }

            // Tính toán số tiền đặt cọc tối thiểu dựa trên tỷ lệ phần trăm
            var minimumDepositAmount = booking.InitialDeposit;
            if (minimumDepositAmount == 0)
            {
                minimumDepositAmount = await CalculateMinimumDepositAsync(booking, cancellationToken);
                booking.SetInitialDeposit(minimumDepositAmount);
            }

            // Xử lý đặt cọc nếu có
            if (request.Booking.DepositAmount > 0)
            {
                // Kiểm tra nếu số tiền đặt cọc ít hơn số tiền tối thiểu
                if (request.Booking.DepositAmount < minimumDepositAmount)
                {
                    throw new ApplicationException($"Số tiền đặt cọc phải ít nhất là {minimumDepositAmount} (tỷ lệ đặt cọc tối thiểu của sân)");
                }
                // Thực hiện đặt cọc
                booking.MakeDeposit(request.Booking.DepositAmount);
            }
            else if (minimumDepositAmount > 0)
            {
                // Nếu sân yêu cầu đặt cọc mà không cung cấp số tiền, ném ngoại lệ
                throw new ApplicationException($"Sân này yêu cầu đặt cọc tối thiểu {minimumDepositAmount}");
            }
            BookingStatus status = booking.Status;
            booking.MarkAsPendingPayment();
            await _bookingRepository.AddBookingAsync(booking, cancellationToken);
            return new CreateBookingResult(booking.Id.Value, status.ToString());
        }

        // Phương thức tính toán số tiền đặt cọc tối thiểu dựa trên tỷ lệ phần trăm
        private async Task<decimal> CalculateMinimumDepositAsync(Booking booking, CancellationToken cancellationToken)
        {
            decimal total = 0;
            foreach (var detail in booking.BookingDetails)
            {
                var court = await _courtRepository.GetCourtByIdAsync(detail.CourtId, cancellationToken);
                var percentage = court?.MinDepositPercentage ?? 100;
                total += detail.TotalPrice * percentage / 100;
            }
            return Math.Round(total, 2);
        }
    }
}