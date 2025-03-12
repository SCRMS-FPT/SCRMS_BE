using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CourtBooking.Application.CourtManagement.Command.DeleteCourtSchedule;

public class DeleteCourtScheduleHandler(IApplicationDbContext _context)
    : IRequestHandler<DeleteCourtScheduleCommand, DeleteCourtScheduleResult>
{
    public async Task<DeleteCourtScheduleResult> Handle(DeleteCourtScheduleCommand request, CancellationToken cancellationToken)
    {
        var scheduleId = CourtScheduleId.Of(request.CourtScheduleId);
        var courtSchedule = await _context.CourtSchedules.FindAsync(new object[] { scheduleId }, cancellationToken);
        if (courtSchedule == null)
        {
            throw new KeyNotFoundException("Court schedule not found");
        }

        _context.CourtSchedules.Remove(courtSchedule);
        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteCourtScheduleResult(true);
    }
}
