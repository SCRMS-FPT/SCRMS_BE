using Coach.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Schedules.GetCoachSchedules
{
    // Query
    public record GetCoachSchedulesQuery(
        Guid CoachId,
        DateOnly StartDate,
        DateOnly EndDate,
        int Page,
        int RecordPerPage) : IQuery<CoachSchedulesResponse>;

    // Response
    public record CoachSchedulesResponse(
        int Page,
        int RecordPerPage,
        int TotalRecords,
        int TotalPages,
        List<ScheduleSlotResponse> Schedules);

    public record ScheduleSlotResponse(
        string Date,
        string StartTime,
        string EndTime,
        string Status);

    public class GetCoachSchedulesQueryHandler : IQueryHandler<GetCoachSchedulesQuery, CoachSchedulesResponse>
    {
        private readonly ICoachScheduleRepository _scheduleRepository;
        private readonly ICoachBookingRepository _bookingRepository;

        public GetCoachSchedulesQueryHandler(
            ICoachScheduleRepository scheduleRepository,
            ICoachBookingRepository bookingRepository)
        {
            _scheduleRepository = scheduleRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<CoachSchedulesResponse> Handle(GetCoachSchedulesQuery query, CancellationToken cancellationToken)
        {
            // Lấy lịch làm việc hàng tuần của coach
            var weeklySchedules = await _scheduleRepository.GetCoachSchedulesByCoachIdAsync(query.CoachId, cancellationToken);

            // Lấy các booking trong khoảng thời gian
            var allBookings = await _bookingRepository.GetCoachBookingsByCoachIdAsync(query.CoachId, cancellationToken);
            var bookings = allBookings.Where(b => b.BookingDate >= query.StartDate && b.BookingDate <= query.EndDate).ToList();

            var allSlots = new List<ScheduleSlotResponse>();

            // Tạo danh sách slot cho từng ngày trong khoảng thời gian
            for (var date = query.StartDate; date <= query.EndDate; date = date.AddDays(1))
            {
                // Ánh xạ DayOfWeek: .NET (0=Sunday, 6=Saturday) -> CoachSchedule (1=Sunday, 7=Saturday)
                int dayOfWeek = date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;

                var dailySchedules = weeklySchedules.Where(s => s.DayOfWeek == dayOfWeek).ToList();
                foreach (var schedule in dailySchedules)
                {
                    string slotDate = date.ToString("yyyy-MM-dd");
                    string startTime = schedule.StartTime.ToString("HH:mm:ss");
                    string endTime = schedule.EndTime.ToString("HH:mm:ss");

                    // Kiểm tra xem slot có bị booked không
                    bool isBooked = bookings.Any(b =>
                        b.BookingDate == date &&
                        b.StartTime < schedule.EndTime &&
                        b.EndTime > schedule.StartTime);

                    string status = isBooked ? "booked" : "available";
                    allSlots.Add(new ScheduleSlotResponse(slotDate, startTime, endTime, status));
                }
            }

            // Sắp xếp và phân trang
            var orderedSlots = allSlots.OrderBy(s => s.Date).ThenBy(s => s.StartTime).ToList();
            int totalRecords = orderedSlots.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / query.RecordPerPage);
            var paginatedSlots = orderedSlots
                .Skip((query.Page - 1) * query.RecordPerPage)
                .Take(query.RecordPerPage)
                .ToList();

            return new CoachSchedulesResponse(
                query.Page,
                query.RecordPerPage,
                totalRecords,
                totalPages,
                paginatedSlots);
        }
    }
}