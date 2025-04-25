using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using CourtBooking.Application.DTOs;
using System.Text.Json;
using BuildingBlocks.Pagination;
using CourtBooking.Application.CourtManagement.Queries.GetSportCenters;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

public class GetSportCentersHandler(IApplicationDbContext _context)
    : IQueryHandler<GetSportCentersQuery, GetSportCentersResult>
{
    public async Task<GetSportCentersResult> Handle(GetSportCentersQuery query, CancellationToken cancellationToken)
    {
        var pageIndex = query.PaginationRequest.PageIndex;
        var pageSize = query.PaginationRequest.PageSize;

        // Start with basic query
        var sportCentersQuery = _context.SportCenters.AsQueryable();

        // Only include non-deleted sport centers
        sportCentersQuery = sportCentersQuery.Where(sc => !sc.IsDeleted);

        // Exclude sport centers owned by the specified owner (if user is CourtOwner)
        if (query.ExcludeOwnerId.HasValue)
        {
            sportCentersQuery = sportCentersQuery.Where(sc => sc.OwnerId != OwnerId.Of(query.ExcludeOwnerId.Value));
        }

        // Apply basic filters
        if (!string.IsNullOrEmpty(query.City))
        {
            sportCentersQuery = sportCentersQuery.Where(sc => sc.Address.City.ToLower() == query.City.ToLower());
        }

        if (!string.IsNullOrEmpty(query.Name))
        {
            sportCentersQuery = sportCentersQuery.Where(sc => sc.Name.ToLower().Contains(query.Name.ToLower()));
        }

        // Get IDs of sport centers that have courts matching the sport type (if specified)
        if (query.SportId.HasValue)
        {
            var sportId = SportId.Of(query.SportId.Value);
            sportCentersQuery = sportCentersQuery.Where(sc =>
                sc.Courts.Any(c => c.SportId == sportId));
        }

        // Filter by availability on specific date and time if requested
        if (query.BookingDate.HasValue)
        {
            var requestedDate = query.BookingDate.Value.Date;
            // Map .NET DayOfWeek (0=Sunday) → DayOfWeekValue (1=Monday…7=Sunday)
            var dow = (int)requestedDate.DayOfWeek;
            var dayValue = dow == 0 ? 7 : dow;

            // Base bookings on that date, non‑cancelled and not payment‑failed
            var baseBookingQ = _context.BookingDetails
                .Join(_context.Bookings,
                      bd => bd.BookingId,
                      b => b.Id,
                      (bd, b) => new { bd, b })
                .Where(x => x.b.Status != BookingStatus.Cancelled
                         && x.b.Status != BookingStatus.PaymentFail
                         && x.b.BookingDate.Date == requestedDate);

            // Apply time‑window filter if provided
            if (query.StartTime.HasValue && query.EndTime.HasValue)
            {
                var start = query.StartTime.Value;
                var end = query.EndTime.Value;
                baseBookingQ = baseBookingQ
                    .Where(x => x.bd.EndTime > start && x.bd.StartTime < end);
            }

            // 1) Load schedules and court durations separately, then filter & join in memory
            var allSchedules = await _context.CourtSchedules
                .ToListAsync(cancellationToken);
            var courtDurations = await _context.Courts
                .Select(c => new { c.Id, c.SlotDuration })
                .ToListAsync(cancellationToken);

            // apply day‐of‐week and time filters in memory
            var filteredSchedules = allSchedules
                .Where(cs => cs.DayOfWeek.Days.Contains(dayValue)
                          && (!query.StartTime.HasValue || cs.EndTime > query.StartTime.Value)
                          && (!query.EndTime.HasValue || cs.StartTime < query.EndTime.Value))
                .ToList();

            // join with durations
            var rawSchedules = filteredSchedules
                .Join(courtDurations,
                      cs => cs.CourtId,
                      cd => cd.Id,
                      (cs, cd) => new
                      {
                          cs.CourtId,
                          cs.StartTime,
                          cs.EndTime,
                          cd.SlotDuration
                      })
                .ToList();

            // 2) Compute overlap & slot counts as before
            var schedules = rawSchedules.Select(x =>
            {
                var windowStart = query.StartTime.HasValue && x.StartTime < query.StartTime.Value
                    ? query.StartTime.Value : x.StartTime;
                var windowEnd = query.EndTime.HasValue && x.EndTime > query.EndTime.Value
                    ? query.EndTime.Value : x.EndTime;
                var overlapMin = Math.Max(0, (windowEnd - windowStart).TotalMinutes);
                return new
                {
                    x.CourtId,
                    OverlapMinutes = (int)overlapMin,
                    SlotDurationMinutes = (int)x.SlotDuration.TotalMinutes
                };
            }).ToList();

            // 3) Sum slots per court
            var slotCounts = schedules
                .GroupBy(x => x.CourtId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.OverlapMinutes / x.SlotDurationMinutes)
                );

            // 2) Count booked slots per court
            var bookedSlots = await baseBookingQ
                .GroupBy(x => x.bd.CourtId)
                .Select(g => new { CourtId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            // 3) Fully‑booked courts = booked count ≥ total slots
            var fullyBookedCourtIds = bookedSlots
                .Where(b => slotCounts.TryGetValue(b.CourtId, out var total) && b.Count >= total)
                .Select(b => b.CourtId)
                .ToList();

            // 4) Keep centers that have at least one court NOT fully booked
            sportCentersQuery = sportCentersQuery
                .Where(sc => sc.Courts.Any(c => !fullyBookedCourtIds.Contains(c.Id)));
        }

        // Get total count from filtered query
        var totalCount = await sportCentersQuery.LongCountAsync(cancellationToken);

        // Get paginated results
        var sportCenters = await sportCentersQuery
            .OrderBy(sc => sc.Name)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .Include(sc => sc.Courts)
            .ToListAsync(cancellationToken);

        // Get sport names
        var sportIds = sportCenters.SelectMany(sc => sc.Courts)
            .Select(c => c.SportId)
            .Distinct()
            .ToList();

        var sportNames = await _context.Sports
            .Where(s => sportIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

        // Map to DTOs
        var sportCenterDtos = sportCenters.Select(sportCenter =>
        {
            // Map courts for each sport center
            var courtDtos = sportCenter.Courts.Select(court => new CourtListDTO(
                Id: court.Id.Value,
                Name: court.CourtName.Value,
                SportId: court.SportId.Value,
                SportName: sportNames.GetValueOrDefault(court.SportId, "Unknown Sport"),
                IsActive: court.Status == CourtStatus.Open,
                Description: court.Description ?? string.Empty,
                MinDepositPercentage: court.MinDepositPercentage
            )).ToList();

            // Create sport center DTO with courts included
            return new SportCenterListDTO(
                Id: sportCenter.Id.Value,
                Name: sportCenter.Name,
                PhoneNumber: sportCenter.PhoneNumber,
                SportNames: sportCenter.Courts
                    .Select(c => sportNames.GetValueOrDefault(c.SportId, "Unknown Sport"))
                    .Distinct()
                    .ToList(),
                Address: sportCenter.Address.ToString(),
                Description: sportCenter.Description,
                Avatar: sportCenter.Images.Avatar.ToString(),
                ImageUrl: sportCenter.Images.ImageUrls.Select(i => i.ToString()).ToList(),
                IsDeleted: sportCenter.IsDeleted,
                Courts: courtDtos  // Include courts in the response
            );
        }).ToList();

        return new GetSportCentersResult(new PaginatedResult<SportCenterListDTO>(pageIndex, pageSize, totalCount, sportCenterDtos));
    }
}