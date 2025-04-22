using BuildingBlocks.Pagination;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Application.Extensions;
using CourtBooking.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourts;

public class GetCourtsHandler : IQueryHandler<GetCourtsQuery, GetCourtsResult>
{
    private readonly ICourtRepository _courtRepository;
    private readonly ISportRepository _sportRepository;
    private readonly ICourtPromotionRepository _promotionRepository;

    public GetCourtsHandler(
        ICourtRepository courtRepository,
        ISportRepository sportRepository,
        ICourtPromotionRepository promotionRepository)
    {
        _courtRepository = courtRepository;
        _sportRepository = sportRepository;
        _promotionRepository = promotionRepository;
    }

    public async Task<GetCourtsResult> Handle(GetCourtsQuery query, CancellationToken cancellationToken)
    {
        int pageIndex = query.PaginationRequest.PageIndex;
        int pageSize = query.PaginationRequest.PageSize;

        // Get paginated courts
        var courts = await _courtRepository.GetPaginatedCourtsAsync(pageIndex, pageSize, cancellationToken);

        // Apply filters if any
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

        // Fix: Create dictionary safely without null keys
        var sportNames = sports
            .Where(s => s != null && s.Id != null)
            .ToDictionary(s => s.Id, s => s.Name);

        // Dictionary to store promotions for each court
        var courtPromotions = new Dictionary<CourtId, List<CourtPromotionDTO>>();

        // Fetch promotions for each court
        foreach (var court in courts)
        {
            var promotions = await _promotionRepository.GetPromotionsByCourtIdAsync(court.Id, cancellationToken);

            // Convert to DTOs
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

            courtPromotions[court.Id] = promotionDtos;
        }

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
            SportCenterName: null, // Can be enhanced if needed
            Promotions: courtPromotions.ContainsKey(court.Id) ? courtPromotions[court.Id] : null,
            CreatedAt: court.CreatedAt,
            LastModified: court.LastModified,
            MinDepositPercentage: court.MinDepositPercentage,
            CancellationWindowHours: court.CancellationWindowHours,
            RefundPercentage: court.RefundPercentage
        )).ToList();

        return new GetCourtsResult(new PaginatedResult<CourtDTO>(pageIndex, pageSize, totalCount, courtDtos));
    }
}