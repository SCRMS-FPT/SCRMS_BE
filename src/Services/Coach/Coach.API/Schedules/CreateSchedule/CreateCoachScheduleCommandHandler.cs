using Microsoft.EntityFrameworkCore;
using Coach.API.Data;
using System.Security.Claims;
using BuildingBlocks.Exceptions;
using Coach.API.Bookings.CreateBooking;
using MediatR;
using Coach.API.Schedules.CreateSchedule;

namespace Coach.API.Schedules.AddSchedule
{
    public record CreateCoachScheduleCommand(
        Guid CoachUserId,
        int DayOfWeek,
        TimeOnly StartTime,
        TimeOnly EndTime) : ICommand<CreateCoachScheduleResult>;

    public record CreateCoachScheduleResult(Guid Id);

    public class CreateCoachScheduleCommandValidator : AbstractValidator<CreateCoachScheduleCommand>
    {
        public CreateCoachScheduleCommandValidator()
        {
            RuleFor(x => x.DayOfWeek)
                 .InclusiveBetween(1, 7)
                 .WithMessage("Day of the week must be between 1 (Monday) and 7 (Sunday)");

            RuleFor(x => x.StartTime)
                .LessThan(x => x.EndTime)
                .WithMessage("Start time must be before end time");
        }
    }

    internal class CreateCoachScheduleCommandHandler : ICommandHandler<CreateCoachScheduleCommand, CreateCoachScheduleResult>
    {
        private readonly CoachDbContext context;
        private readonly IMediator mediator;

        public CreateCoachScheduleCommandHandler(CoachDbContext context, IMediator mediator)
        {
            this.context = context;
            this.mediator = mediator;
        }

        public async Task<CreateCoachScheduleResult> Handle(
            CreateCoachScheduleCommand command,
            CancellationToken cancellationToken)
        {
            var coach = await context.Coaches
                .FirstOrDefaultAsync(c => c.UserId == command.CoachUserId);

            if (coach == null)
                throw new NotFoundException("User is not registered as a coach");

            var hasConflict = await context.CoachSchedules
             .AnyAsync(s =>
                 s.CoachId == command.CoachUserId &&
                 s.DayOfWeek == command.DayOfWeek &&
                 (
                     (command.StartTime >= s.StartTime && command.StartTime < s.EndTime) ||
                     (command.EndTime > s.StartTime && command.EndTime <= s.EndTime) ||
                     (command.StartTime <= s.StartTime && command.EndTime >= s.EndTime)
                 ), cancellationToken);

            if (hasConflict)
                throw new AlreadyExistsException("The schedule conflicts with an existing schedule.");

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

            await mediator.Publish(new ScheduleCreatedEvent(schedule.Id, schedule.CoachId), cancellationToken);

            return new CreateCoachScheduleResult(schedule.Id);
        }
    }
}