using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;

namespace CourtBooking.Application.CourtManagement.Command.UpdateCourtSchedule;

public class UpdateCourtScheduleHandler : IRequestHandler<UpdateCourtScheduleCommand, UpdateCourtScheduleResult>
{
    private readonly ICourtScheduleRepository _courtScheduleRepository;

    public UpdateCourtScheduleHandler(ICourtScheduleRepository courtScheduleRepository)
    {
        _courtScheduleRepository = courtScheduleRepository;
    }

    public async Task<UpdateCourtScheduleResult> Handle(UpdateCourtScheduleCommand request, CancellationToken cancellationToken)
    {
        var scheduleId = CourtScheduleId.Of(request.CourtSchedule.Id);
        var courtSchedule = await _courtScheduleRepository.GetCourtScheduleByIdAsync(scheduleId, cancellationToken);
        if (courtSchedule == null)
        {
            throw new KeyNotFoundException("Court schedule not found");
        }

        courtSchedule.Update(
            DayOfWeekValue.Of(request.CourtSchedule.DayOfWeek),
            request.CourtSchedule.StartTime,
            request.CourtSchedule.EndTime,
            request.CourtSchedule.PriceSlot,
            (CourtScheduleStatus)request.CourtSchedule.Status
        );

        await _courtScheduleRepository.UpdateCourtScheduleAsync(courtSchedule, cancellationToken);
        return new UpdateCourtScheduleResult(true);
    }
}