using BuildingBlocks.Pagination;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using System.Text.Json;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourtsByOwner;

public class GetCourtsByOwnerHandler : IQueryHandler<GetCourtsByOwnerQuery, GetCourtsByOwnerResult>
{
    private readonly ICourtRepository _courtRepository;
    private readonly ISportCenterRepository _sportCenterRepository;
    private readonly ISportRepository _sportRepository;
    private readonly ICourtPromotionRepository _promotionRepository;

    public GetCourtsByOwnerHandler(
        ICourtRepository courtRepository,
        ISportCenterRepository sportCenterRepository,
        ISportRepository sportRepository,
        ICourtPromotionRepository promotionRepository)
    {
        _courtRepository = courtRepository;
        _sportCenterRepository = sportCenterRepository;
        _sportRepository = sportRepository;
        _promotionRepository = promotionRepository;
    }

    public async Task<GetCourtsByOwnerResult> Handle(GetCourtsByOwnerQuery query, CancellationToken cancellationToken)
    {
        int pageIndex = query.PaginationRequest.PageIndex;
        int pageSize = query.PaginationRequest.PageSize;

        // Get sport centers owned by this owner
        var sportCenters = await _sportCenterRepository.GetSportCentersByOwnerIdAsync(query.OwnerId, cancellationToken);
        var sportCenterIds = sportCenters.Select(sc => sc.Id).ToList();

        // Get all courts in these sport centers
        var courts = await _courtRepository.GetCourtsBySportCenterIdsAsync(sportCenterIds, cancellationToken);

        // Apply filters if any
        if (query.SportId.HasValue)
        {
            courts = courts.Where(c => c.SportId.Value == query.SportId.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(query.CourtType))
        {
            courts = courts.Where(c => c.CourtType.ToString().Equals(query.CourtType, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Apply pagination
        var totalCount = courts.Count;
        var paginatedCourts = courts
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Get sport names
        var sportIds = paginatedCourts.Select(c => c.SportId).Distinct().ToList();
        var sports = await _sportRepository.GetSportsByIdsAsync(sportIds, cancellationToken);
        var sportNames = sports.ToDictionary(s => s.Id, s => s.Name);

        // Create a dictionary for sport center names
        var sportCenterNames = sportCenters.ToDictionary(sc => sc.Id, sc => sc.Name);

        // Get promotions for each court
        var courtPromotions = new Dictionary<CourtId, List<CourtPromotionDTO>>();
        foreach (var court in paginatedCourts)
        {
            var promotions = await _promotionRepository.GetPromotionsByCourtIdAsync(court.Id, cancellationToken);

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

        // Map to DTOs
        var courtDtos = paginatedCourts.Select(court => new CourtDTO(
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
            SportCenterName: sportCenterNames.GetValueOrDefault(court.SportCenterId, "Unknown Center"),
            Promotions: courtPromotions.ContainsKey(court.Id) ? courtPromotions[court.Id] : null,
            CreatedAt: court.CreatedAt,
            LastModified: court.LastModified,
            MinDepositPercentage: court.MinDepositPercentage,
            CancellationWindowHours: court.CancellationWindowHours,
            RefundPercentage: court.RefundPercentage
        )).ToList();

        return new GetCourtsByOwnerResult(new PaginatedResult<CourtDTO>(pageIndex, pageSize, totalCount, courtDtos));
    }
}