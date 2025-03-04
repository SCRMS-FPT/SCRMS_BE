using Microsoft.EntityFrameworkCore;
using Coach.API.Data;
using System.Security.Claims;
using BuildingBlocks.Exceptions;
using Coach.API.Bookings.CreateBooking;
using MediatR;
using Coach.API.Schedules.CreateSchedule;
using Coach.API.Data.Repositories;

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
        private readonly ICoachRepository _coachRepository;
        private readonly ICoachScheduleRepository _scheduleRepository;
        private readonly CoachDbContext _context;
        private readonly IMediator _mediator;

        public CreateCoachScheduleCommandHandler(
            ICoachRepository coachRepository,
            ICoachScheduleRepository scheduleRepository,
            CoachDbContext context,
            IMediator mediator)
        {
            _coachRepository = coachRepository;
            _scheduleRepository = scheduleRepository;
            _context = context;
            _mediator = mediator;
        }

        public async Task<CreateCoachScheduleResult> Handle(
            CreateCoachScheduleCommand command,
            CancellationToken cancellationToken)
        {
            var coach = await _coachRepository.GetCoachByIdAsync(command.CoachUserId, cancellationToken);
            if (coach == null)
                throw new NotFoundException("User is not registered as a coach");

            var hasConflict = await _scheduleRepository.HasCoachScheduleConflictAsync(
                command.CoachUserId,
                command.DayOfWeek,
                command.StartTime,
                command.EndTime,
                cancellationToken);

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

            await _scheduleRepository.AddCoachScheduleAsync(schedule, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new ScheduleCreatedEvent(schedule.Id, schedule.CoachId), cancellationToken);

            return new CreateCoachScheduleResult(schedule.Id);
        }
    }
}