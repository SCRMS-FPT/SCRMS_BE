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
        var pageIndex = query.PaginationRequest.PageIndex;
        var pageSize = query.PaginationRequest.PageSize;

        // Lấy danh sách Court
        var totalCount = await _courtRepository.GetTotalCourtCountAsync(cancellationToken);
        var courts = await _courtRepository.GetPaginatedCourtsAsync(pageIndex, pageSize, cancellationToken);

        // Lấy danh sách SportId từ các Court
        var sportIds = courts.Select(c => c.SportId).Distinct().ToList();

        // Lấy thông tin Sport
        var sports = await _sportRepository.GetSportsByIdsAsync(sportIds, cancellationToken);
        var sportNames = sports.ToDictionary(s => s.Id, s => s.Name);

        // Ánh xạ dữ liệu sang CourtDTO, bao gồm SportName
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
            SportCenterName: null, // Có thể thêm logic nếu cần
            CreatedAt: court.CreatedAt,
            LastModified: court.LastModified
        )).ToList();

        return new GetCourtsResult(new PaginatedResult<CourtDTO>(pageIndex, pageSize, totalCount, courtDtos));
    }
}