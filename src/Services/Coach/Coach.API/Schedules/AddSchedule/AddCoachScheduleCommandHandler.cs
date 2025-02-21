using Microsoft.EntityFrameworkCore;
using Coach.API.Data;
using System.Security.Claims;
using BuildingBlocks.Exceptions;

namespace Coach.API.Schedules.AddSchedule
{
    public record AddCoachScheduleCommand(
        Guid CoachUserId,
        int DayOfWeek,
        TimeSpan StartTime,
        TimeSpan EndTime) : ICommand<AddCoachScheduleResult>;

    public record AddCoachScheduleResult(Guid Id);

    public class AddCoachScheduleCommandValidator : AbstractValidator<AddCoachScheduleCommand>
    {
        public AddCoachScheduleCommandValidator()
        {
            RuleFor(x => x.DayOfWeek).InclusiveBetween(1, 7);
            RuleFor(x => x.StartTime)
                .LessThan(x => x.EndTime)
                .WithMessage("Start time must be before end time");
        }
    }

    internal class AddCoachScheduleCommandHandler(CoachDbContext context)
    : ICommandHandler<AddCoachScheduleCommand, AddCoachScheduleResult>
    {
        public async Task<AddCoachScheduleResult> Handle(
            AddCoachScheduleCommand command,
            CancellationToken cancellationToken)
        {
            // Tìm kiếm huấn luyện viên bằng UserID (từ JWT token)
            var coach = await context.Coaches
                .FirstOrDefaultAsync(c => c.UserId == command.CoachUserId);

            if (coach == null)
                throw new NotFoundException("User is not registered as a coach");

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