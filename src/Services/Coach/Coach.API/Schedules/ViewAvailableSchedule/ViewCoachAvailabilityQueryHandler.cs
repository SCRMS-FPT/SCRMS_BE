using Coach.API.Data;
using Coach.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Schedules.ViewAvailableSchedule
{
    public record ViewCoachAvailabilityQuery(Guid CoachUserId, int Page, int RecordPerPage) : IQuery<List<AvailableScheduleSlot>>;
    public record AvailableScheduleSlot(int DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);

    public class ViewCoachAvailabilityCommandValidator : AbstractValidator<ViewCoachAvailabilityQuery>
    {
        public ViewCoachAvailabilityCommandValidator()
        {
            RuleFor(x => x.CoachUserId).NotEmpty();
        }
    }

    internal class ViewCoachAvailabilityQueryHandler : IQueryHandler<ViewCoachAvailabilityQuery, List<AvailableScheduleSlot>>
    {
        private readonly ICoachScheduleRepository _scheduleRepository;
        private readonly ICoachBookingRepository _bookingRepository;

        public ViewCoachAvailabilityQueryHandler(
            ICoachScheduleRepository scheduleRepository,
            ICoachBookingRepository bookingRepository)
        {
            _scheduleRepository = scheduleRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<List<AvailableScheduleSlot>> Handle(ViewCoachAvailabilityQuery query, CancellationToken cancellationToken)
        {
            var coachSchedules = await _scheduleRepository.GetCoachSchedulesByCoachIdAsync(query.CoachUserId, cancellationToken);
            var bookedSchedules = await _bookingRepository.GetCoachBookingsByCoachIdAsync(query.CoachUserId, cancellationToken);

            var availableSlots = new List<AvailableScheduleSlot>();

            foreach (var schedule in coachSchedules)
            {
                bool isBooked = bookedSchedules.Any(b =>
                    (int)b.BookingDate.DayOfWeek + 1 == schedule.DayOfWeek &&
                    b.StartTime < schedule.EndTime &&
                    b.EndTime > schedule.StartTime);

                if (!isBooked)
                {
                    availableSlots.Add(new AvailableScheduleSlot(schedule.DayOfWeek, schedule.StartTime, schedule.EndTime));
                }
            }

            return availableSlots.Skip((query.Page - 1) * query.RecordPerPage).Take(query.RecordPerPage).ToList();
        }
    }
}