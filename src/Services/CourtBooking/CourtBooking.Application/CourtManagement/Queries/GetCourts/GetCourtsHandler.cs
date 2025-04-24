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
        // Get all courts first
        var allCourts = await _courtRepository.GetAllCourtsAsync(cancellationToken);

        // Apply filters
        if (query.sportCenterId.HasValue)
        {
            allCourts = allCourts.Where(c => c.SportCenterId == SportCenterId.Of(query.sportCenterId.Value)).ToList();
        }
        if (query.sportId.HasValue)
        {
            allCourts = allCourts.Where(c => c.SportId == SportId.Of(query.sportId.Value)).ToList();
        }
        if (!string.IsNullOrWhiteSpace(query.courtType))
        {
            allCourts = allCourts.Where(c => c.CourtType.ToString().Equals(query.courtType, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Calculate total count after applying filters
        long totalCount = allCourts.Count;

        // Apply pagination after filtering
        int pageIndex = query.PaginationRequest.PageIndex;
        int pageSize = query.PaginationRequest.PageSize;

        var courts = allCourts
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToList();

        // Load related data
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