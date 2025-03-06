using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CourtBooking.Application.CourtManagement.Command.CreateCourtSchedule;

public class CreateCourtScheduleHandler(IApplicationDbContext _context)
    : IRequestHandler<CreateCourtScheduleCommand, CreateCourtScheduleResult>
{
    public async Task<CreateCourtScheduleResult> Handle(CreateCourtScheduleCommand request, CancellationToken cancellationToken)
    {
        var courtId = CourtId.Of(request.CourtSchedule.CourtId);
        var court = await _context.Courts.FindAsync( courtId, cancellationToken);
        if (court == null)
        {
            throw new KeyNotFoundException("Court not found");
        }

        var newSchedule = CourtSchedule.Create(
            CourtScheduleId.Of(Guid.NewGuid()),
            courtId,
            DayOfWeekValue.Of(request.CourtSchedule.DayOfWeek),
            request.CourtSchedule.StartTime,
            request.CourtSchedule.EndTime,
            request.CourtSchedule.PriceSlot
        );

        court.AddCourtSlot(courtId, request.CourtSchedule.DayOfWeek, request.CourtSchedule.StartTime, request.CourtSchedule.EndTime, request.CourtSchedule.PriceSlot);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateCourtScheduleResult(newSchedule.Id.Value);
    }
}
