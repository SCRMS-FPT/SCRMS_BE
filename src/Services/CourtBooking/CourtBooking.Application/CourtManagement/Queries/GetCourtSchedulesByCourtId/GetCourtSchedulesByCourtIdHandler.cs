using MediatR;
using CourtBooking.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using CourtBooking.Domain.Models;
using CourtBooking.Application.CourtManagement.Queries.GetCourtSchedulesByCourtId;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourtSlotsByCourtName;

public class GetCourtSchedulesByCourtIdHandler(IApplicationDbContext _context)
    : IRequestHandler<GetCourtSchedulesByCourtIdQuery, GetCourtSchedulesByCourtIdResult>
{
    public async Task<GetCourtSchedulesByCourtIdResult> Handle(GetCourtSchedulesByCourtIdQuery query, CancellationToken cancellationToken)
    {
        var court = await _context.Courts
            .Include(c => c.CourtSchedules)
            .FirstOrDefaultAsync(c => c.Id == CourtId.Of(query.CourtId), cancellationToken);

        if (court == null)
        {
            throw new KeyNotFoundException("Court not found");
        }

        var courtSchedules = court.CourtSchedules.Select(slot => new CourtScheduleDTO(
            CourtId: slot.CourtId.Value,
            DayOfWeek: slot.DayOfWeek.Days.ToArray(),
            StartTime: slot.StartTime,
            EndTime: slot.EndTime,
            PriceSlot: slot.PriceSlot,
            Status: (int)slot.Status
        )).ToList();

        return new GetCourtSchedulesByCourtIdResult(courtSchedules);
    }
}
