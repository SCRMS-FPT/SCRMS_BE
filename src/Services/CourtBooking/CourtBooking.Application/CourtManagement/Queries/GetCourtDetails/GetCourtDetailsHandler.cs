// language: csharp
using MediatR;
using CourtBooking.Application.Data;
using CourtBooking.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using CourtBooking.Domain.ValueObjects;
using System.Text.Json;
using CourtBooking.Domain.Models;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourtDetails;

public class GetCourtDetailsHandler(IApplicationDbContext _context) : IQueryHandler<GetCourtDetailsQuery, GetCourtDetailsResult>
{
    public async Task<GetCourtDetailsResult> Handle(GetCourtDetailsQuery query, CancellationToken cancellationToken)
    {
        var courtId = CourtId.Of(query.CourtId);

        var court = await _context.Courts
            .Include(c => c.CourtSchedules)
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

        // Get court promotions
        var promotions = await _context.CourtPromotions
            .Where(p => p.CourtId == courtId)
            .ToListAsync(cancellationToken);

        var promotionDtos = promotions.Select(p => new CourtPromotionDTO(
            Id: p.Id.Value,
            CourtId: p.CourtId.Value,
            Description: p.Description,
            DiscountType: p.DiscountType,
            DiscountValue: p.DiscountValue,
            ValidFrom: p.ValidFrom,
            ValidTo: p.ValidTo,
            CreatedAt: p.CreatedAt,
            LastModified: p.LastModified
        )).ToList();

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
            Promotions: promotionDtos,
            CreatedAt: court.CreatedAt,
            LastModified: court.LastModified,
            MinDepositPercentage: court.MinDepositPercentage,
            CancellationWindowHours: court.CancellationWindowHours,
            RefundPercentage: court.RefundPercentage
        );

        return new GetCourtDetailsResult(courtDto);
    }
}