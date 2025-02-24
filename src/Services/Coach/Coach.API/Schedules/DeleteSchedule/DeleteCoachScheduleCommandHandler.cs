using BuildingBlocks.Exceptions;
using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Schedules.DeleteSchedule
{
    public record DeleteScheduleCommand(Guid ScheduleId,
        Guid CoachId) : ICommand<DeleteScheduleResult>;

    public record DeleteScheduleResult(Boolean IsDeleted);

    public class DeleteScheduleCommandValidator : AbstractValidator<DeleteScheduleCommand>
    {
        public DeleteScheduleCommandValidator()
        {
            RuleFor(x => x.ScheduleId).NotEmpty().WithMessage("ScheduleId is required.");
        }
    }
    internal class DeleteScheduleCommandHandler
       : ICommandHandler<DeleteScheduleCommand, DeleteScheduleResult>
    {
        private readonly CoachDbContext context;
        private readonly IMediator mediator;

        public DeleteScheduleCommandHandler(CoachDbContext context, IMediator mediator)
        {
            this.context = context;
            this.mediator = mediator;
        }

        public async Task<DeleteScheduleResult> Handle(DeleteScheduleCommand command, CancellationToken cancellationToken)
        {
            var schedule = await context.CoachSchedules
                .FirstOrDefaultAsync(s => s.Id == command.ScheduleId, cancellationToken);

            if (schedule == null)
                throw new NotFoundException("Schedule not found.");

            if (schedule.CoachId != command.CoachId)
                throw new UnauthorizedAccessException("You are not authorized to delete this schedule.");

            // This checking seem very tricky, need to be careful about this
            bool hasBookings = await context.CoachBookings
                .AnyAsync(b => b.CoachId == schedule.CoachId &&
                               b.StartTime >= schedule.StartTime &&
                               b.EndTime <= schedule.EndTime, cancellationToken);

            if (hasBookings)
                throw new AlreadyExistsException("Cannot delete the schedule as it has existing bookings.");

            //TODO: This is an hard delete, in the future you will need to modify this code
            context.CoachSchedules.Remove(schedule);
            await context.SaveChangesAsync(cancellationToken);

            return new DeleteScheduleResult(true);
        }
    }
}
