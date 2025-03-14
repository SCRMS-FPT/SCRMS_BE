using BuildingBlocks.Pagination;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourts;

public class GetCourtsHandler : IQueryHandler<GetCourtsQuery, GetCourtsResult>
{
    private readonly ICourtRepository _courtRepository;
    private readonly ISportRepository _sportRepository;

    public GetCourtsHandler(ICourtRepository courtRepository, ISportRepository sportRepository)
    {
        _courtRepository = courtRepository;
        _sportRepository = sportRepository;
    }

    public async Task<GetCourtsResult> Handle(GetCourtsQuery query, CancellationToken cancellationToken)
    {
        int pageIndex = query.PaginationRequest.PageIndex;
        int pageSize = query.PaginationRequest.PageSize;

        // Lấy danh sách các Court (giả định không có filter trực tiếp trong repository)
        var courts = await _courtRepository.GetPaginatedCourtsAsync(pageIndex, pageSize, cancellationToken);

        // Áp dụng các filter nếu có
        if (query.sportCenterId.HasValue)
        {
            courts = courts.Where(c => c.SportCenterId.Value == query.sportCenterId.Value).ToList();
        }
        if (query.sportId.HasValue)
        {
            courts = courts.Where(c => c.SportId.Value == query.sportId.Value).ToList();
        }
        if (!string.IsNullOrWhiteSpace(query.courtType))
        {
            courts = courts.Where(c => c.CourtType.ToString().Equals(query.courtType, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        long totalCount = courts.Count;

        var sportIds = courts.Select(c => c.SportId).Distinct().ToList();
        var sports = await _sportRepository.GetSportsByIdsAsync(sportIds, cancellationToken);
        var sportNames = sports.ToDictionary(s => s.Id, s => s.Name);

        var courtDtos = courts.Select(court => new CourtDTO(
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
            SportCenterName: null, // Có thể bổ sung logic nếu cần
            CreatedAt: court.CreatedAt,
            LastModified: court.LastModified
        )).ToList();

        return new GetCourtsResult(new PaginatedResult<CourtDTO>(pageIndex, pageSize, totalCount, courtDtos));
    }
}