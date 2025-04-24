using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourtAvailability
{
    public class GetCourtAvailabilityHandler : IRequestHandler<GetCourtAvailabilityQuery, GetCourtAvailabilityResult>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICourtRepository _courtRepository;
        private readonly ICourtScheduleRepository _courtScheduleRepository;
        private readonly ICourtPromotionRepository _courtPromotionRepository;
        private readonly IBookingRepository _bookingRepository;

        public GetCourtAvailabilityHandler(
            IApplicationDbContext context,
            ICourtRepository courtRepository,
            ICourtScheduleRepository courtScheduleRepository,
            ICourtPromotionRepository courtPromotionRepository,
            IBookingRepository bookingRepository)
        {
            _context = context;
            _courtRepository = courtRepository;
            _courtScheduleRepository = courtScheduleRepository;
            _courtPromotionRepository = courtPromotionRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<GetCourtAvailabilityResult> Handle(GetCourtAvailabilityQuery request, CancellationToken cancellationToken)
        {
            // Kiểm tra sân tồn tại
            var court = await _courtRepository.GetCourtByIdAsync(CourtId.Of(request.CourtId), cancellationToken);
            if (court == null)
                throw new DomainException($"Không tìm thấy sân với ID: {request.CourtId}");

            // Kiểm tra khoảng thời gian hợp lệ (tối đa 31 ngày)
            if (request.EndDate < request.StartDate)
                throw new DomainException("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu");

            var daysDifference = (request.EndDate - request.StartDate).Days + 1;
            if (daysDifference > 31)
                throw new DomainException("Khoảng thời gian không được vượt quá 31 ngày");

            // Lấy thông tin slotDuration từ court
            int slotDuration = (int)court.SlotDuration.TotalMinutes; // Mặc định 60 phút nếu không có

            // Danh sách các ngày cần kiểm tra
            var daysToCheck = Enumerable.Range(0, daysDifference)
                .Select(offset => request.StartDate.AddDays(offset))
                .ToList();

            // Lấy thông tin lịch trình của sân
            var courtSchedules = await _courtScheduleRepository.GetSchedulesByCourt(court.Id, cancellationToken);

            // Lấy thông tin khuyến mãi của sân
            var courtPromotions = await _courtPromotionRepository.GetValidPromotionsForCourtAsync(
                court.Id, request.StartDate, request.EndDate, cancellationToken);

            // Lấy thông tin booking hiện có
            var bookings = await _bookingRepository.GetBookingsInDateRangeForCourtAsync(
                request.CourtId, request.StartDate, request.EndDate, cancellationToken);

            // Tạo lịch trình cho từng ngày
            var schedules = new List<DailySchedule>();
            foreach (var date in daysToCheck)
            {
                int dayOfWeek = ((int)date.DayOfWeek + 6) % 7 + 1; // Chuyển từ 0-6 (Chủ Nhật = 0) sang 1-7 (Thứ 2 = 1)

                // Lấy lịch của ngày trong tuần - kiểm tra DayOfWeek.Days chứa giá trị tương ứng
                var scheduleForDay = courtSchedules
                    .Where(s => s.DayOfWeek.Days.Contains(dayOfWeek))
                    .ToList();

                if (!scheduleForDay.Any())
                {
                    // Không có lịch cho ngày này
                    schedules.Add(new DailySchedule(date, dayOfWeek, new List<TimeSlot>()));
                    continue;
                }

                var timeSlots = new List<TimeSlot>();
                foreach (var schedule in scheduleForDay)
                {
                    // Tính toán các slot trong schedule này
                    var start = schedule.StartTime;
                    var end = schedule.EndTime;
                    var basePrice = schedule.PriceSlot;

                    // Tạo các slot với khoảng thời gian là slotDuration
                    for (var time = start; time < end; time = time.Add(TimeSpan.FromMinutes(slotDuration)))
                    {
                        // Tính thời gian kết thúc của slot
                        var slotEndTime = time.Add(TimeSpan.FromMinutes(slotDuration));
                        if (slotEndTime > end) slotEndTime = end;

                        // Kiểm tra xem slot đã được đặt chưa
                        string status = "AVAILABLE";
                        string? bookedBy = null;

                        // Tìm booking cho slot và ngày này
                        foreach (var booking in bookings)
                        {
                            // Chỉ xem xét các booking không ở trạng thái cancel
                            if (booking.Status != Domain.Enums.BookingStatus.Cancelled && booking.Status != Domain.Enums.BookingStatus.PaymentFail)
                            {
                                var matchingBookingDetail = booking.BookingDetails
                                    .Where(bd =>
                                        bd.CourtId.Value == request.CourtId &&
                                        DateOnly.FromDateTime(booking.BookingDate) == DateOnly.FromDateTime(date) &&
                                        bd.StartTime <= time && bd.EndTime >= slotEndTime)
                                    .FirstOrDefault();

                                if (matchingBookingDetail != null)
                                {
                                    status = "BOOKED";
                                    bookedBy = booking.UserId.Value.ToString();
                                    break;
                                }
                            }
                        }

                        // Kiểm tra nếu lịch đang bảo trì
                        if (schedule.Status == Domain.Enums.CourtScheduleStatus.Maintenance)
                        {
                            status = "MAINTENANCE";
                        }

                        // Tìm khuyến mãi áp dụng cho slot này
                        PromotionInfo? promotion = null;
                        var validPromotion = courtPromotions
                            .Where(p => p.ValidFrom <= date && p.ValidTo >= date)
                            .OrderByDescending(p => p.DiscountValue)
                            .FirstOrDefault();

                        if (validPromotion != null)
                        {
                            promotion = new PromotionInfo(
                                validPromotion.DiscountType.ToString(),
                                validPromotion.DiscountValue
                            );
                        }

                        // Thêm slot vào danh sách
                        timeSlots.Add(new TimeSlot(
                            time.ToString("hh\\:mm"),
                            slotEndTime.ToString("hh\\:mm"),
                            basePrice,
                            status,
                            promotion,
                            status == "BOOKED" ? bookedBy : null
                        ));
                    }
                }

                // Thêm lịch của ngày vào kết quả
                schedules.Add(new DailySchedule(date, dayOfWeek, timeSlots));
            }

            return new GetCourtAvailabilityResult(
                request.CourtId,
                slotDuration,
                schedules
            );
        }
    }
}