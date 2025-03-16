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

namespace CourtBooking.Application.BookingManagement.Command.CreateBooking
{
    public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, CreateBookingResult>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ICourtScheduleRepository _courtScheduleRepository;
        private readonly ICourtRepository _courtRepository;

        public CreateBookingHandler(
            IBookingRepository bookingRepository,
            ICourtScheduleRepository courtScheduleRepository,
            ICourtRepository courtRepository)
        {
            _bookingRepository = bookingRepository;
            _courtScheduleRepository = courtScheduleRepository;
            _courtRepository = courtRepository;
        }

        public async Task<CreateBookingResult> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            var userId = UserId.Of(request.Booking.UserId);

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

                var bookingDayOfWeekInt = (int)request.Booking.BookingDate.DayOfWeek + 1;
                var allCourtSchedules = await _courtScheduleRepository.GetSchedulesByCourtIdAsync(courtId, cancellationToken);
                var schedules = allCourtSchedules
                    .Where(s => s.DayOfWeek.Days.Contains(bookingDayOfWeekInt))
                    .ToList();
                if (!schedules.Any())
                {
                    throw new ApplicationException($"No schedules found for court {courtId.Value} on day {bookingDayOfWeekInt}");
                }

                // Truyền tham số MinDepositPercentage từ sân vào phương thức AddBookingDetail
                booking.AddBookingDetail(courtId, detail.StartTime, detail.EndTime, schedules, court.MinDepositPercentage);
            }

            // Tính toán số tiền đặt cọc tối thiểu dựa trên tỷ lệ phần trăm
            var minimumDepositAmount = CalculateMinimumDepositAmount(booking);

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

            await _bookingRepository.AddBookingAsync(booking, cancellationToken);
            return new CreateBookingResult(booking.Id.Value);
        }

        // Phương thức tính toán số tiền đặt cọc tối thiểu dựa trên tỷ lệ phần trăm
        private decimal CalculateMinimumDepositAmount(Booking booking)
        {
            // Giả sử các chi tiết booking có mức % đặt cọc khác nhau (nếu đặt nhiều sân khác nhau)
            var depositAmounts = booking.BookingDetails
                .Select(detail => new
                {
                    Price = detail.TotalPrice,
                    MinDepositPercentage = _courtRepository.GetCourtByIdAsync(detail.CourtId, CancellationToken.None).Result?.MinDepositPercentage ?? 100
                })
                .ToList();

            decimal minimumDepositAmount = 0;

            foreach (var item in depositAmounts)
            {
                minimumDepositAmount += item.Price * item.MinDepositPercentage / 100;
            }

            return Math.Round(minimumDepositAmount, 2);
        }
    }
}