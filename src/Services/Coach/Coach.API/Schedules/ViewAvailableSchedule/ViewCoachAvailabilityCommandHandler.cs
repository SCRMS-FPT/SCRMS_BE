using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Schedules.ViewAvailableSchedule
{
    public record ViewCoachAvailabilityCommand(Guid CoachUserId, int Page, int RecordPerPage) : ICommand<List<AvailableScheduleSlot>>;
    public record AvailableScheduleSlot(int DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);
    public class ViewCoachAvailabilityCommandValidator : AbstractValidator<ViewCoachAvailabilityCommand>
    {
        public ViewCoachAvailabilityCommandValidator()
        {
            RuleFor(x => x.CoachUserId).NotEmpty();
        }
    }
    internal class ViewCoachAvailabilityCommandHandler
        : ICommandHandler<ViewCoachAvailabilityCommand, List<AvailableScheduleSlot>>
    {
        private readonly CoachDbContext context;
        private readonly IMediator mediator;

        public ViewCoachAvailabilityCommandHandler(CoachDbContext context, IMediator mediator)
        {
            this.context = context;
            this.mediator = mediator;
        }

        public async Task<List<AvailableScheduleSlot>> Handle(ViewCoachAvailabilityCommand command, CancellationToken cancellationToken)
        {
            var coachSchedules = await context.CoachSchedules
                .Where(cs => cs.CoachId == command.CoachUserId)
                .ToListAsync(cancellationToken);

            var bookedSchedules = await context.CoachBookings
                .Where(b => b.CoachId == command.CoachUserId && b.BookingDate >= DateOnly.FromDateTime(DateTime.UtcNow))
                .Select(b => new { b.BookingDate, b.StartTime, b.EndTime })
                .ToListAsync(cancellationToken);

            var availableSlots = new List<AvailableScheduleSlot>();

            foreach (var schedule in coachSchedules)
            {
                bool isBooked = bookedSchedules.Any(b =>
                    (int)b.BookingDate.DayOfWeek == schedule.DayOfWeek &&
                    b.StartTime < schedule.EndTime &&
                    b.EndTime > schedule.StartTime);

                if (!isBooked)
                {
                    availableSlots.Add(new AvailableScheduleSlot(schedule.DayOfWeek, schedule.StartTime, schedule.EndTime));
                }
            }

            return availableSlots.Skip((command.Page - 1) * command.RecordPerPage).Take(command.RecordPerPage).ToList();
        }
    }
}
