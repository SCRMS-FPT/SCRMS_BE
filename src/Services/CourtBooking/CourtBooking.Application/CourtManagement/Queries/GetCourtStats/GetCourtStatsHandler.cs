using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourtStats
{
    public class GetCourtStatsHandler : IRequestHandler<GetCourtStatsQuery, GetCourtStatsResult>
    {
        private readonly ICourtRepository _courtRepository;
        private readonly IBookingRepository _bookingRepository;

        public GetCourtStatsHandler(ICourtRepository courtRepository, IBookingRepository bookingRepository)
        {
            _courtRepository = courtRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<GetCourtStatsResult> Handle(GetCourtStatsQuery request, CancellationToken cancellationToken)
        {
            // Lấy tổng số sân
            var totalCourts = await _courtRepository.GetTotalCourtCountAsync(cancellationToken);

            // Tính tổng doanh thu từ đặt sân
            var bookingsQuery = _bookingRepository.GetBookingsQuery();
            
            // Chỉ tính các đơn đã được xác nhận/thanh toán (không tính đơn hủy)
            bookingsQuery = bookingsQuery.Where(b => b.Status != BookingStatus.Cancelled);

            // Áp dụng điều kiện lọc theo thời gian nếu có
            if (request.StartDate.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate >= request.StartDate.Value);
            }
            
            if (request.EndDate.HasValue)
            {
                // Đảm bảo lấy đến hết ngày kết thúc
                var endDateWithTime = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate <= endDateWithTime);
            }

            // Tính tổng doanh thu
            var totalRevenue = await bookingsQuery.SumAsync(b => b.TotalPaid, cancellationToken);

            return new GetCourtStatsResult
            {
                TotalCourts = totalCourts,
                TotalCourtsRevenue = totalRevenue,
                DateRange = new DateRange
                {
                    StartDate = request.StartDate?.ToString("yyyy-MM-dd"),
                    EndDate = request.EndDate?.ToString("yyyy-MM-dd")
                }
            };
        }
    }
} 