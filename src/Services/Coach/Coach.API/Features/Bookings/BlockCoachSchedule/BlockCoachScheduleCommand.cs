using MediatR;

namespace Coach.API.Features.Bookings.BlockCoachSchedule
{
    public record BlockCoachScheduleCommand(
        Guid CoachId,
        Guid SportId,
        DateOnly BookingDate,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string Notes
    ) : ICommand<BlockCoachScheduleResult>;

    public record BlockCoachScheduleResult(Guid BookingId);
}