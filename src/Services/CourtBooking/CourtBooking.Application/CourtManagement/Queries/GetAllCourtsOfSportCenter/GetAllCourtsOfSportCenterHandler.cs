using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CourtBooking.Application.CourtManagement.Queries.GetAllCourtsOfSportCenter;

public class GetAllCourtsOfSportCenterHandler : IQueryHandler<GetAllCourtsOfSportCenterQuery, GetAllCourtsOfSportCenterResult>
{
    private readonly ICourtRepository _courtRepository;
    private readonly ISportRepository _sportRepository;

    public GetAllCourtsOfSportCenterHandler(
        ICourtRepository courtRepository,
        ISportRepository sportRepository)
    {
        _courtRepository = courtRepository;
        _sportRepository = sportRepository;
    }

    public async Task<GetAllCourtsOfSportCenterResult> Handle(GetAllCourtsOfSportCenterQuery query, CancellationToken cancellationToken)
    {
        var sportCenterId = SportCenterId.Of(query.SportCenterId);
        var courts = await _courtRepository.GetAllCourtsOfSportCenterAsync(sportCenterId, cancellationToken);

        var sportIds = courts.Select(c => c.SportId).Distinct().ToList();
        var sports = await _sportRepository.GetAllSportsAsync(cancellationToken);
        var sportNames = sports.Where(s => sportIds.Contains(s.Id)).ToDictionary(s => s.Id, s => s.Name);

        var courtDtos = courts.Select(court =>
        {
            List<FacilityDTO>? facilities = null;
            if (!string.IsNullOrEmpty(court.Facilities))
            {
                facilities = JsonSerializer.Deserialize<List<FacilityDTO>>(court.Facilities);
            }

            return new CourtDTO(
                Id: court.Id.Value,
                CourtName: court.CourtName.Value,
                SportId: court.SportId.Value,
                SportCenterId: court.SportCenterId.Value,
                Description: court.Description,
                Facilities: facilities,
                SlotDuration: court.SlotDuration,
                Status: court.Status,
                CourtType: court.CourtType,
                SportName: sportNames.GetValueOrDefault(court.SportId, "Unknown Sport"),
                SportCenterName: null,
                CreatedAt: court.CreatedAt,
                LastModified: court.LastModified,
                MinDepositPercentage: court.MinDepositPercentage
            );
        }).ToList();

        return new GetAllCourtsOfSportCenterResult(courtDtos);
    }
}