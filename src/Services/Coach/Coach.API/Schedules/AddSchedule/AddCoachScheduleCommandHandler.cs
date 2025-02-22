using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Schedules.AddSchedule
{
    public record AddCoachScheduleCommand(
        Guid CoachUserId,
        int DayOfWeek,
        TimeOnly StartTime,
        TimeOnly EndTime) : ICommand<AddCoachScheduleResult>;

    public record AddCoachScheduleResult(Guid Id);

    public class AddCoachScheduleCommandValidator : AbstractValidator<AddCoachScheduleCommand>
    {
        public AddCoachScheduleCommandValidator()
        {
            RuleFor(x => x.DayOfWeek).InclusiveBetween(1, 7);
            RuleFor(x => x.StartTime).LessThan(x => x.EndTime);
        }
    }

    internal class AddCoachScheduleCommandHandler : ICommandHandler<AddCoachScheduleCommand, AddCoachScheduleResult>
    {
        private readonly CoachDbContext context;

        public AddCoachScheduleCommandHandler(CoachDbContext context)
        {
            this.context = context;
        }

        public async Task<AddCoachScheduleResult> Handle(AddCoachScheduleCommand command, CancellationToken cancellationToken)
        {
            var coach = await context.Coaches
                .FirstOrDefaultAsync(c => c.UserId == command.CoachUserId, cancellationToken);

            if (coach == null)
                throw new Exception("User is not registered as a coach");

            var schedule = new CoachSchedule
            {
                Id = Guid.NewGuid(),
                CoachId = command.CoachUserId,
                DayOfWeek = command.DayOfWeek,
                StartTime = command.StartTime,
                EndTime = command.EndTime,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.CoachSchedules.Add(schedule);
            await context.SaveChangesAsync(cancellationToken);

            return new AddCoachScheduleResult(schedule.Id);
        }
    }
}