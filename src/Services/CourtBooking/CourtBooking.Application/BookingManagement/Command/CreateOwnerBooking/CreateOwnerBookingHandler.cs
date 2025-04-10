using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourtBooking.Application.Data.Repositories;
using BuildingBlocks.Messaging.Outbox;
using CourtBooking.Domain.Enums;

namespace CourtBooking.Application.BookingManagement.Command.CreateOwnerBooking
{
    public class CreateOwnerBookingHandler : IRequestHandler<CreateOwnerBookingCommand, CreateOwnerBookingResult>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ICourtScheduleRepository _courtScheduleRepository;
        private readonly ICourtRepository _courtRepository;
        private readonly ISportCenterRepository _sportCenterRepository;
        private readonly IOutboxService _outboxService;

        public CreateOwnerBookingHandler(
            IBookingRepository bookingRepository,
            ICourtScheduleRepository courtScheduleRepository,
            ICourtRepository courtRepository,
            ISportCenterRepository sportCenterRepository,
            IOutboxService outboxService)
        {
            _bookingRepository = bookingRepository;
            _courtScheduleRepository = courtScheduleRepository;
            _courtRepository = courtRepository;
            _sportCenterRepository = sportCenterRepository;
            _outboxService = outboxService;
        }

        public async Task<CreateOwnerBookingResult> Handle(CreateOwnerBookingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Xác thực chủ sân có quyền quản lý các sân đang được đặt
                foreach (var detail in request.Booking.BookingDetails)
                {
                    var courtId = CourtId.Of(detail.CourtId);
                    var court = await _courtRepository.GetCourtByIdAsync(courtId, cancellationToken);

                    if (court == null)
                    {
                        return new CreateOwnerBookingResult(
                            Guid.Empty,
                            "Failed",
                            false,
                            $"Sân với ID {detail.CourtId} không tồn tại");
                    }

                    // Kiểm tra xem chủ sân có quyền quản lý sân này không
                    var sportCenter = await _sportCenterRepository.GetSportCenterByIdAsync(court.SportCenterId, cancellationToken);

                    if (sportCenter == null || sportCenter.OwnerId.Value != request.OwnerId)
                    {
                        return new CreateOwnerBookingResult(
                            Guid.Empty,
                            "Failed",
                            false,
                            $"Bạn không có quyền quản lý sân với ID {detail.CourtId}");
                    }
                }

                var userId = UserId.Of(request.OwnerId);

                // Tạo booking với trạng thái hoàn thành trực tiếp
                var booking = Booking.Create(
                    id: BookingId.Of(Guid.NewGuid()),
                    userId: userId,
                    bookingDate: request.Booking.BookingDate,
                    note: request.Note
                );

                // Thêm chi tiết booking
                foreach (var detail in request.Booking.BookingDetails)
                {
                    var courtId = CourtId.Of(detail.CourtId);
                    var court = await _courtRepository.GetCourtByIdAsync(courtId, cancellationToken);

                    // Kiểm tra lịch hoạt động của sân trong ngày được đặt
                    var bookingDayOfWeekInt = (int)request.Booking.BookingDate.DayOfWeek + 1;
                    var allCourtSchedules = await _courtScheduleRepository.GetSchedulesByCourtIdAsync(courtId, cancellationToken);
                    var schedules = allCourtSchedules
                        .Where(s => s.DayOfWeek.Days.Contains(bookingDayOfWeekInt))
                        .ToList();

                    if (!schedules.Any())
                    {
                        return new CreateOwnerBookingResult(
                            Guid.Empty,
                            "Failed",
                            false,
                            $"Không tìm thấy lịch hoạt động cho sân {court.CourtName.Value} vào ngày {request.Booking.BookingDate.DayOfWeek}");
                    }

                    // Kiểm tra xem slot đã được đặt chưa
                    var existingBookings = await _bookingRepository.GetBookingsInDateRangeForCourtAsync(
                        detail.CourtId,
                        request.Booking.BookingDate,
                        request.Booking.BookingDate,
                        cancellationToken);

                    var conflictingBooking = existingBookings
                        .Where(b => b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.PaymentFail)
                        .SelectMany(b => b.BookingDetails)
                        .Where(bd =>
                            bd.CourtId.Value == detail.CourtId &&
                            ((bd.StartTime <= detail.StartTime && bd.EndTime > detail.StartTime) ||
                             (bd.StartTime < detail.EndTime && bd.EndTime >= detail.EndTime) ||
                             (bd.StartTime >= detail.StartTime && bd.EndTime <= detail.EndTime)))
                        .FirstOrDefault();

                    if (conflictingBooking != null)
                    {
                        return new CreateOwnerBookingResult(
                            Guid.Empty,
                            "Failed",
                            false,
                            $"Khung giờ từ {detail.StartTime} đến {detail.EndTime} đã được đặt");
                    }

                    // Thêm chi tiết booking (không áp dụng khuyến mãi khi chủ sân đặt)
                    booking.AddBookingDetail(courtId, detail.StartTime, detail.EndTime, schedules);
                }

                // Đánh dấu booking là đã hoàn thành (đã thanh toán đầy đủ)
                booking.MarkAsCompleted();

                // Lưu booking vào database
                await _bookingRepository.AddBookingAsync(booking, cancellationToken);

                // Trả về kết quả thành công
                return new CreateOwnerBookingResult(
                    booking.Id.Value,
                    booking.Status.ToString(),
                    true,
                    "Đánh dấu sân đã được đặt thành công");
            }
            catch (Exception ex)
            {
                return new CreateOwnerBookingResult(
                    Guid.Empty,
                    "Failed",
                    false,
                    $"Lỗi khi đánh dấu sân đã đặt: {ex.Message}");
            }
        }
    }
}