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
            var requestedDate = query.BookingDate.Value.Date; // Ensure we're comparing dates only

            // Get all bookings for the requested date
            var bookedCourtsQuery = _context.BookingDetails
            .Join(_context.Bookings,
                bd => bd.BookingId,
                b => b.Id,
                (bd, b) => new { BookingDetail = bd, Booking = b })
            .Where(x => x.Booking.Status != BookingStatus.Cancelled &&
                  x.Booking.BookingDate.Date == requestedDate);

            if (query.StartTime.HasValue)
            {
                var startTime = query.StartTime.Value;
                bookedCourtsQuery = bookedCourtsQuery.Where(x =>
                    (x.BookingDetail.StartTime <= startTime && x.BookingDetail.EndTime > startTime) ||
                    (x.BookingDetail.StartTime >= startTime));
            }

            if (query.EndTime.HasValue)
            {
                var endTime = query.EndTime.Value;
                bookedCourtsQuery = bookedCourtsQuery.Where(x =>
                    (x.BookingDetail.StartTime < endTime && x.BookingDetail.EndTime >= endTime) ||
                    (x.BookingDetail.EndTime <= endTime));
            }

            var bookedCourts = await bookedCourtsQuery
            .Select(x => x.BookingDetail.CourtId)
            .Distinct()
            .ToListAsync(cancellationToken);

            // Filter sport centers that have courts not booked on the requested date and time
            sportCentersQuery = sportCentersQuery.Where(sc =>
            sc.Courts.Any(c => !bookedCourts.Contains(c.Id)));
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
                Courts: courtDtos  // Include courts in the response
            );
        }).ToList();

        return new GetSportCentersResult(new PaginatedResult<SportCenterListDTO>(pageIndex, pageSize, totalCount, sportCenterDtos));
    }
}