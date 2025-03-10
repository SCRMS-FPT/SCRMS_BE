using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using CourtBooking.Application.DTOs;
using System.Text.Json;
using BuildingBlocks.Pagination;
using CourtBooking.Application.CourtManagement.Queries.GetSportCenters;

public class GetSportCentersHandler(IApplicationDbContext _context)
    : IQueryHandler<GetSportCentersQuery, GetSportCentersResult>
{
    public async Task<GetSportCentersResult> Handle(GetSportCentersQuery query, CancellationToken cancellationToken)
    {
        var pageIndex = query.PaginationRequest.PageIndex;
        var pageSize = query.PaginationRequest.PageSize;

        // Tổng số sport centers
        var totalCount = await _context.SportCenters.LongCountAsync(cancellationToken);

        // Lấy danh sách sport centers theo trang
        var sportCenters = await _context.SportCenters
            .OrderBy(sc => sc.Name)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .Include(sc => sc.Courts)
            .ToListAsync(cancellationToken);

        // Lấy danh sách SportId duy nhất từ toàn bộ SportCenters đang phân trang
        var sportIds = sportCenters.SelectMany(sc => sc.Courts)
            .Select(c => c.SportId)
            .Distinct()
            .ToList();

        // Truy vấn SportName tương ứng với SportId
        var sportNames = await _context.Sports
            .Where(s => sportIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

        // Map dữ liệu sang DTO
        var sportCenterDtos = sportCenters.Select(sportCenter =>
            new SportCenterDTO(
                Id: sportCenter.Id.Value,
                Name: sportCenter.Name,
                PhoneNumber: sportCenter.PhoneNumber,
                Address: sportCenter.Address.ToString(),
                Description: sportCenter.Description,
                Courts: sportCenter.Courts.Select(court =>
                {
                    List<FacilityDTO>? facilities = null;
                    if (!string.IsNullOrEmpty(court.Facilities))
                    {
                        try
                        {
                            facilities = JsonSerializer.Deserialize<List<FacilityDTO>>(court.Facilities);
                        }
                        catch (JsonException)
                        {
                            facilities = new List<FacilityDTO>();
                        }
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
                        SportName: sportNames.GetValueOrDefault(court.SportId, "Unknown Sport"),
                        SportCenterName: sportCenter.Name,
                        CreatedAt: court.CreatedAt,
                        LastModified: court.LastModified
                    );
                }).ToList()
            )
        ).ToList();

        return new GetSportCentersResult(new PaginatedResult<SportCenterDTO>(pageIndex, pageSize, totalCount, sportCenterDtos));
    }
}
