using BuildingBlocks.Pagination;
using CourtBooking.Application.DTOs;

namespace CourtBooking.Application.CourtManagement.Queries.GetSportCenters;

public record GetSportCentersQuery(
    PaginationRequest PaginationRequest,
    string? City = null,
    string? Name = null
) : IQuery<GetSportCentersResult>;

public record GetSportCentersResult(PaginatedResult<SportCenterListDTO> SportCenters);