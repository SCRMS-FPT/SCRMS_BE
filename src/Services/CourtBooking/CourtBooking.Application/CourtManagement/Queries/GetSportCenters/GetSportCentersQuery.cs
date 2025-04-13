using BuildingBlocks.Pagination;
using CourtBooking.Application.DTOs;
using System;

namespace CourtBooking.Application.CourtManagement.Queries.GetSportCenters;

public record GetSportCentersQuery(
    PaginationRequest PaginationRequest,
    string? City = null,
    string? Name = null,
    Guid? SportId = null,
    DateTime? BookingDate = null,
    TimeSpan? StartTime = null,
    TimeSpan? EndTime = null,
    Guid? ExcludeOwnerId = null
) : IQuery<GetSportCentersResult>;

public record GetSportCentersResult(PaginatedResult<SportCenterListDTO> SportCenters);