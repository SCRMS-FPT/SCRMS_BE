using BuildingBlocks.Pagination;
using CourtBooking.Application.DTOs;
using System;
using BuildingBlocks.CQRS;

namespace CourtBooking.Application.CourtManagement.Queries.GetSportCentersByOwner;

public record GetSportCentersByOwnerQuery(
    Guid OwnerId,
    PaginationRequest PaginationRequest,
    string? City = null,
    string? Name = null,
    Guid? SportId = null,
    DateTime? BookingDate = null,
    TimeSpan? StartTime = null,
    TimeSpan? EndTime = null
) : IQuery<GetSportCentersByOwnerResult>;

public record GetSportCentersByOwnerResult(PaginatedResult<SportCenterListDTO> SportCenters);