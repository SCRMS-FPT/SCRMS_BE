using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using CourtBooking.Application.DTOs;
using System.Text.Json;
using BuildingBlocks.Pagination;
using CourtBooking.Application.CourtManagement.Queries.GetSportCenters;
using CourtBooking.Application.Data.Repositories;

public class GetSportCentersHandler : IQueryHandler<GetSportCentersQuery, GetSportCentersResult>
{
    private readonly ISportCenterRepository _sportCenterRepository;
    private readonly ISportRepository _sportRepository;

    public GetSportCentersHandler(
        ISportCenterRepository sportCenterRepository,
        ISportRepository sportRepository)
    {
        _sportCenterRepository = sportCenterRepository;
        _sportRepository = sportRepository;
    }

    public async Task<GetSportCentersResult> Handle(GetSportCentersQuery query, CancellationToken cancellationToken)
    {
        var pageIndex = query.PaginationRequest.PageIndex;
        var pageSize = query.PaginationRequest.PageSize;

        var totalCount = await _sportCenterRepository.GetTotalSportCenterCountAsync(cancellationToken);
        var sportCenters = await _sportCenterRepository.GetPaginatedSportCentersAsync(pageIndex, pageSize, cancellationToken);

        var sportIds = sportCenters.SelectMany(sc => sc.Courts).Select(c => c.SportId).Distinct().ToList();
        var sports = await _sportRepository.GetAllSportsAsync(cancellationToken);
        var sportNames = sports.Where(s => sportIds.Contains(s.Id)).ToDictionary(s => s.Id, s => s.Name);

        var sportCenterDtos = sportCenters.Select(sportCenter => new SportCenterDTO(
            Id: sportCenter.Id.Value,
            Name: sportCenter.Name,
            PhoneNumber: sportCenter.PhoneNumber,
            Address: sportCenter.Address.ToString(),
            Description: sportCenter.Description,
            Courts: sportCenter.Courts.Select(court => new CourtDTO(
                Id: court.Id.Value,
                CourtName: court.CourtName.Value,
                SportId: court.SportId.Value,
                SportCenterId: court.SportCenterId.Value,
                Description: court.Description,
                Facilities: court.Facilities != null ? JsonSerializer.Deserialize<List<FacilityDTO>>(court.Facilities) : null,
                SlotDuration: court.SlotDuration,
                Status: court.Status,
                CourtType: court.CourtType,
                SportName: sportNames.GetValueOrDefault(court.SportId, "Unknown Sport"),
                SportCenterName: sportCenter.Name,
                CreatedAt: court.CreatedAt,
                LastModified: court.LastModified
            )).ToList()
        )).ToList();

        return new GetSportCentersResult(new PaginatedResult<SportCenterDTO>(pageIndex, pageSize, totalCount, sportCenterDtos));
    }
}