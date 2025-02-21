
using BuildingBlocks.Pagination;
using CourtBooking.Application.DTOs;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourts
{
    public record GetCourtsQuery(PaginationRequest PaginationRequest) : IQuery<GetCourtsResult>;

    public record GetCourtsResult(PaginatedResult<CourtDTO> Courts);
}
