using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Schedules.UpdateSchedule
{
    public class UpdateCoachScheduleCommandHandler
    {
        public record UpdateScheduleCommand(
            Guid ScheduleId,
            Guid CoachId,
            int DayOfWeek,
            TimeOnly StartTime,
            TimeOnly EndTime
        ) : ICommand<UpdateScheduleResult>;

        public record UpdateScheduleResult(Guid Id);

        public class UpdateScheduleCommandValidator : AbstractValidator<UpdateScheduleCommand>
        {
            public UpdateScheduleCommandValidator()
            {
                RuleFor(x => x.ScheduleId).NotEmpty();
                RuleFor(x => x.CoachId).NotEmpty();
                RuleFor(x => x.DayOfWeek).InclusiveBetween(1,7);
                RuleFor(x => x.StartTime).LessThan(x => x.EndTime);
            }
        }

        internal class UpdateScheduleCommandHandler : ICommandHandler<UpdateScheduleCommand, UpdateScheduleResult>
        {
            private readonly CoachDbContext context;
            private readonly IMediator mediator;

            public UpdateScheduleCommandHandler(CoachDbContext context, IMediator mediator)
            {
                this.context = context;
                this.mediator = mediator;
            }

            public async Task<UpdateScheduleResult> Handle(UpdateScheduleCommand command, CancellationToken cancellationToken)
            {
                var schedule = await context.CoachSchedules
                    .FirstOrDefaultAsync(s => s.Id == command.ScheduleId, cancellationToken);

                if (schedule == null)
                {
                    throw new ScheduleNotFoundException(command.ScheduleId);
                }

                if (schedule.CoachId != command.CoachId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to update this schedule.");
                }

                var hasConflict = await context.CoachSchedules
                    .AnyAsync(s =>
                        s.CoachId == command.CoachId &&
                        s.Id != command.ScheduleId &&
                        s.DayOfWeek == command.DayOfWeek &&
                        (
                            (command.StartTime >= s.StartTime && command.StartTime < s.EndTime) ||
                            (command.EndTime > s.StartTime && command.EndTime <= s.EndTime) ||
                            (command.StartTime <= s.StartTime && command.EndTime >= s.EndTime)
                        ),
                        cancellationToken);

                if (hasConflict)
                {
                    throw new ScheduleConflictException("The updated schedule conflicts with an existing schedule.");
                }

                schedule.DayOfWeek = command.DayOfWeek;
                schedule.StartTime = command.StartTime;
                schedule.EndTime = command.EndTime;
                schedule.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync(cancellationToken);

                await mediator.Publish(new ScheduleUpdatedEvent(schedule.Id, schedule.CoachId), cancellationToken);

                return new UpdateScheduleResult(schedule.Id);
            }
        }

        public class ScheduleUpdatedEventHandler : INotificationHandler<ScheduleUpdatedEvent>
        {
            public async Task Handle(ScheduleUpdatedEvent notification, CancellationToken cancellationToken)
            {
                // Nothing
            }
        }
    }
}
