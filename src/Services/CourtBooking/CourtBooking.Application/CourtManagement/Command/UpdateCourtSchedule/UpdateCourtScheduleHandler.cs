using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;
namespace CourtBooking.Application.CourtManagement.Command.UpdateCourtSchedule;

public class UpdateCourtScheduleHandler(IApplicationDbContext _context)
    : IRequestHandler<UpdateCourtScheduleCommand, UpdateCourtScheduleResult>
{

    public async Task<UpdateCourtScheduleResult> Handle(UpdateCourtScheduleCommand request, CancellationToken cancellationToken)
    {
        var scheduleId = CourtScheduleId.Of(request.CourtSchedule.Id);
        var courtSchedule = await _context.CourtSlots.FindAsync(scheduleId, cancellationToken);
        if (courtSchedule == null)
        {
            throw new KeyNotFoundException("Court schedule not found");
        }

        courtSchedule.Update(
            DayOfWeekValue.Of(request.CourtSchedule.DayOfWeek),
            request.CourtSchedule.StartTime,
            request.CourtSchedule.EndTime,
            request.CourtSchedule.PriceSlot,
            (CourtSlotStatus)request.CourtSchedule.Status
        );

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateCourtScheduleResult(true);
    }
}
