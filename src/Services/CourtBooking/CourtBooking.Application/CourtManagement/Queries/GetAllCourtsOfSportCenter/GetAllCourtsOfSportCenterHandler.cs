using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class GetAllCourtsOfSportCenterHandler(IApplicationDbContext _context) : IQueryHandler<GetAllCourtsOfSportCenterQuery, GetAllCourtsOfSportCenterResult>
{
    public async Task<GetAllCourtsOfSportCenterResult> Handle(GetAllCourtsOfSportCenterQuery query, CancellationToken cancellationToken)
    {
        var sportCenterId = SportCenterId.Of(query.SportCenterId);

        var courts = await _context.Courts
            .Where(c => c.SportCenterId == sportCenterId)
            .ToListAsync(cancellationToken);

        // Lấy tất cả SportId cần thiết trước khi tạo DTO
        var sportIds = courts.Select(c => c.SportId).Distinct().ToList();
        var sportNames = await _context.Sports
            .Where(s => sportIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

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
                SportName: sportNames.GetValueOrDefault(court.SportId, "Unknown Sport"), // Truy xuất nhanh
                SportCenterName: null,
                CreatedAt: court.CreatedAt,
                LastModified: court.LastModified
            );
        }).ToList();

        return new GetAllCourtsOfSportCenterResult(courtDtos);
    }
}
