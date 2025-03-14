using MediatR;
using CourtBooking.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using CourtBooking.Domain.Models;
using CourtBooking.Application.CourtManagement.Queries.GetCourtSchedulesByCourtId;
using CourtBooking.Application.Data.Repositories;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourtSlotsByCourtName;

public class GetCourtSchedulesByCourtIdHandler : IRequestHandler<GetCourtSchedulesByCourtIdQuery, GetCourtSchedulesByCourtIdResult>
{
    private readonly ICourtRepository _courtRepository;
    private readonly ICourtScheduleRepository _courtScheduleRepository;

    public GetCourtSchedulesByCourtIdHandler(
        ICourtRepository courtRepository,
        ICourtScheduleRepository courtScheduleRepository)
    {
        _courtRepository = courtRepository;
        _courtScheduleRepository = courtScheduleRepository;
    }

    public async Task<GetCourtSchedulesByCourtIdResult> Handle(GetCourtSchedulesByCourtIdQuery query, CancellationToken cancellationToken)
    {
        var courtId = CourtId.Of(query.CourtId);
        var court = await _courtRepository.GetCourtByIdAsync(courtId, cancellationToken);
        if (court == null)
        {
            throw new KeyNotFoundException("Court not found");
        }

        var courtSchedules = await _courtScheduleRepository.GetSchedulesByCourtIdAsync(courtId, cancellationToken);
        var courtScheduleDtos = courtSchedules.Select(slot => new CourtScheduleDTO(
            CourtId: slot.CourtId.Value,
            DayOfWeek: slot.DayOfWeek.Days.ToArray(),
            StartTime: slot.StartTime,
            EndTime: slot.EndTime,
            PriceSlot: slot.PriceSlot,
            Status: (int)slot.Status
        )).ToList();

        return new GetCourtSchedulesByCourtIdResult(courtScheduleDtos);
    }
}