using BuildingBlocks.Exceptions;
using Coach.API.Data;
using Coach.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Schedules.DeleteSchedule
{
    public record DeleteScheduleCommand(Guid ScheduleId,
        Guid CoachId) : ICommand<DeleteScheduleResult>;

    public record DeleteScheduleResult(bool IsDeleted);

    public class DeleteScheduleCommandValidator : AbstractValidator<DeleteScheduleCommand>
    {
        public DeleteScheduleCommandValidator()
        {
            RuleFor(x => x.ScheduleId).NotEmpty().WithMessage("ScheduleId is required.");
        }
    }

    // Changed from internal to public for testing
    public class DeleteScheduleCommandHandler : ICommandHandler<DeleteScheduleCommand, DeleteScheduleResult>
    {
        private readonly ICoachScheduleRepository _scheduleRepository;
        private readonly ICoachBookingRepository _bookingRepository;
        private readonly CoachDbContext _context;

        public DeleteScheduleCommandHandler(
            ICoachScheduleRepository scheduleRepository,
            ICoachBookingRepository bookingRepository,
            CoachDbContext context)
        {
            _scheduleRepository = scheduleRepository;
            _bookingRepository = bookingRepository;
            _context = context;
        }

        public async Task<DeleteScheduleResult> Handle(DeleteScheduleCommand command, CancellationToken cancellationToken)
        {
            var schedule = await _scheduleRepository.GetCoachScheduleByIdAsync(command.ScheduleId, cancellationToken);
            if (schedule == null)
                throw new NotFoundException("Schedule not found.");

            if (schedule.CoachId != command.CoachId)
                throw new UnauthorizedAccessException("You are not authorized to delete this schedule.");

            var hasBookings = await _bookingRepository.HasOverlappingCoachBookingAsync(
                schedule.CoachId,
                DateOnly.FromDateTime(DateTime.UtcNow),
                schedule.StartTime,
                schedule.EndTime,
                cancellationToken);

            if (hasBookings)
                throw new AlreadyExistsException("Cannot delete the schedule as it has existing bookings.");

            await _scheduleRepository.DeleteCoachScheduleAsync(schedule, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new DeleteScheduleResult(true);
        }
    }
}