// language: csharp
using MediatR;
using CourtBooking.Application.Data;
using CourtBooking.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using CourtBooking.Domain.ValueObjects;
using System.Text.Json;
using CourtBooking.Domain.Models;

public class GetCourtDetailsHandler(IApplicationDbContext _context) : IQueryHandler<GetCourtDetailsQuery, GetCourtDetailsResult>
{
    public async Task<GetCourtDetailsResult> Handle(GetCourtDetailsQuery query, CancellationToken cancellationToken)
    {
        var courtId = CourtId.Of(query.CourtId);

        var court = await _context.Courts
            .Include(c => c.CourtSlots)
            .FirstOrDefaultAsync(c => c.Id == courtId, cancellationToken);

        if (court == null)
        {
            throw new KeyNotFoundException("Court not found");
        }

        // Get related sport
        var sport = await _context.Sports
            .FirstOrDefaultAsync(s => s.Id == court.SportId, cancellationToken);

        // Get related sport center
        var sportCenter = await _context.SportCenters
            .FirstOrDefaultAsync(sc => sc.Id == court.SportCenterId, cancellationToken);

        List<FacilityDTO>? facilities = null;
        if (!string.IsNullOrEmpty(court.Facilities))
        {
            facilities = JsonSerializer.Deserialize<List<FacilityDTO>>(court.Facilities);
        }

        var courtDto = new CourtDTO(
            Id: court.Id.Value,
            CourtName: court.CourtName.Value,
            SportId: court.SportId.Value,
            SportCenterId: court.SportCenterId.Value,
            Description: court.Description,
            Facilities: facilities,
            SlotDuration: court.SlotDuration,
            Status: court.Status,
            CourtType: court.CourtType,
            SportName: sport?.Name,
            SportCenterName: sportCenter?.Name,
            CreatedAt: court.CreatedAt,
            LastModified: court.LastModified
        );

        return new GetCourtDetailsResult(courtDto);
    }
}