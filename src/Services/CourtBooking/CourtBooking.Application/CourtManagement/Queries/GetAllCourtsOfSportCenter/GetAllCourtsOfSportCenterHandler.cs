using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CourtBooking.Application.CourtManagement.Queries.GetAllCourtsOfSportCenter;

public class GetAllCourtsOfSportCenterHandler : IQueryHandler<GetAllCourtsOfSportCenterQuery, GetAllCourtsOfSportCenterResult>
{
    private readonly ICourtRepository _courtRepository;
    private readonly ISportRepository _sportRepository;
    private readonly ICourtPromotionRepository _promotionRepository;

    public GetAllCourtsOfSportCenterHandler(
        ICourtRepository courtRepository,
        ISportRepository sportRepository,
        ICourtPromotionRepository promotionRepository)
    {
        _courtRepository = courtRepository;
        _sportRepository = sportRepository;
        _promotionRepository = promotionRepository;
    }

    public async Task<GetAllCourtsOfSportCenterResult> Handle(GetAllCourtsOfSportCenterQuery query, CancellationToken cancellationToken)
    {
        var sportCenterId = SportCenterId.Of(query.SportCenterId);
        var courts = await _courtRepository.GetAllCourtsOfSportCenterAsync(sportCenterId, cancellationToken);

        var sportIds = courts.Select(c => c.SportId).Distinct().ToList();
        var sports = await _sportRepository.GetAllSportsAsync(cancellationToken);
        var sportNames = sports.Where(s => sportIds.Contains(s.Id)).ToDictionary(s => s.Id, s => s.Name);

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
                CourtType: court.CourtType,
                SportName: sportNames.GetValueOrDefault(court.SportId, "Unknown Sport"),
                SportCenterName: null,
                Promotions: courtPromotions.ContainsKey(court.Id) ? courtPromotions[court.Id] : null,
                CreatedAt: court.CreatedAt,
                LastModified: court.LastModified,
                MinDepositPercentage: court.MinDepositPercentage,
            CancellationWindowHours: court.CancellationWindowHours,
            RefundPercentage: court.RefundPercentage
            );
        }).ToList();

        return new GetAllCourtsOfSportCenterResult(courtDtos);
    }
}