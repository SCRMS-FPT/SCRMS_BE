using BuildingBlocks.Pagination;
using CourtBooking.Application.DTOs;

namespace CourtBooking.Application.CourtManagement.Queries.GetSportCenters;

public record GetSportCentersQuery(PaginationRequest PaginationRequest)
    : IQuery<GetSportCentersResult>;

public record GetSportCentersResult(PaginatedResult<SportCenterDTO> SportCenters);
