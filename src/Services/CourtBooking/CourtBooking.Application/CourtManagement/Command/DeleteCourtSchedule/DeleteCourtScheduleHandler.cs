using CourtBooking.Application.Data.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CourtBooking.Application.CourtManagement.Command.DeleteCourtSchedule;

public class DeleteCourtScheduleHandler : IRequestHandler<DeleteCourtScheduleCommand, DeleteCourtScheduleResult>
{
    private readonly ICourtScheduleRepository _courtScheduleRepository;

    public DeleteCourtScheduleHandler(ICourtScheduleRepository courtScheduleRepository)
    {
        _courtScheduleRepository = courtScheduleRepository;
    }

    public async Task<DeleteCourtScheduleResult> Handle(DeleteCourtScheduleCommand request, CancellationToken cancellationToken)
    {
        var scheduleId = CourtScheduleId.Of(request.CourtScheduleId);
        var courtSchedule = await _courtScheduleRepository.GetCourtScheduleByIdAsync(scheduleId, cancellationToken);
        if (courtSchedule == null)
        {
            throw new KeyNotFoundException("Court schedule not found");
        }

        await _courtScheduleRepository.DeleteCourtScheduleAsync(scheduleId, cancellationToken);
        return new DeleteCourtScheduleResult(true);
    }
}