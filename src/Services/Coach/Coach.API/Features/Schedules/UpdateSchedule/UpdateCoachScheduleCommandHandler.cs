using Coach.API.Data;
using Coach.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Schedules.UpdateSchedule
{
    public record UpdateScheduleCommand(
        Guid ScheduleId,
        Guid CoachId,
        int DayOfWeek,
        TimeOnly StartTime,
        TimeOnly EndTime
    ) : ICommand<UpdateScheduleResult>;

    public record UpdateScheduleResult(bool IsUpdated);

    public class UpdateScheduleCommandValidator : AbstractValidator<UpdateScheduleCommand>
    {
        public UpdateScheduleCommandValidator()
        {
            RuleFor(x => x.ScheduleId).NotEmpty();
            RuleFor(x => x.CoachId).NotEmpty();
            RuleFor(x => x.DayOfWeek).InclusiveBetween(1, 7);
            RuleFor(x => x.StartTime).LessThan(x => x.EndTime);
        }
    }

    // Changed from internal to public for testing
    public class UpdateScheduleCommandHandler : ICommandHandler<UpdateScheduleCommand, UpdateScheduleResult>
    {
        private readonly ICoachScheduleRepository _scheduleRepository;
        private readonly CoachDbContext _context;
        private readonly IMediator _mediator;

        public UpdateScheduleCommandHandler(
            ICoachScheduleRepository scheduleRepository,
            CoachDbContext context,
            IMediator mediator)
        {
            _scheduleRepository = scheduleRepository;
            _context = context;
            _mediator = mediator;
        }

        public async Task<UpdateScheduleResult> Handle(UpdateScheduleCommand command, CancellationToken cancellationToken)
        {
            var schedule = await _scheduleRepository.GetCoachScheduleByIdAsync(command.ScheduleId, cancellationToken);
            if (schedule == null)
                throw new ScheduleNotFoundException(command.ScheduleId);

            if (schedule.CoachId != command.CoachId)
                throw new UnauthorizedAccessException("You are not authorized to update this schedule.");

            var hasConflict = await _scheduleRepository.HasCoachScheduleConflictExcludingCurrentAsync(
                command.ScheduleId,
                command.CoachId,
                command.DayOfWeek,
                command.StartTime,
                command.EndTime,
                cancellationToken);

            if (hasConflict)
                throw new ScheduleConflictException("The updated schedule conflicts with an existing schedule.");

            schedule.DayOfWeek = command.DayOfWeek;
            schedule.StartTime = command.StartTime;
            schedule.EndTime = command.EndTime;
            schedule.UpdatedAt = DateTime.UtcNow;

            await _scheduleRepository.UpdateCoachScheduleAsync(schedule, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new ScheduleUpdatedEvent(schedule.Id, schedule.CoachId), cancellationToken);

            return new UpdateScheduleResult(true);
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