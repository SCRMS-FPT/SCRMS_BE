using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.ValueObjects;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourtSchedulesByCourtId;

public class GetCourtSchedulesByCourtIdHandler : IQueryHandler<GetCourtSchedulesByCourtIdQuery, GetCourtSchedulesByCourtIdResult>
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
            throw new NotFoundException($"Court with ID {query.CourtId} not found.");
        }

        var courtSchedules = await _courtScheduleRepository.GetSchedulesByCourtIdAsync(courtId, cancellationToken);
        var filteredSchedules = query.Day.HasValue
            ? courtSchedules.Where(cs => cs.DayOfWeek.Days.Contains(query.Day.Value)).ToList()
            : courtSchedules;

        var courtScheduleDtos = filteredSchedules.Select(cs => new CourtScheduleDTO(
            Id: cs.Id.Value,
            CourtId: cs.CourtId.Value,
            DayOfWeek: cs.DayOfWeek.Days.ToArray(),
            StartTime: cs.StartTime,
            EndTime: cs.EndTime,
            PriceSlot: cs.PriceSlot,
            Status: (int)cs.Status,
            CreatedAt: cs.CreatedAt,
            LastModified: cs.LastModified
        )).ToList();

        return new GetCourtSchedulesByCourtIdResult(courtScheduleDtos);
    }
}