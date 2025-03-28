using BuildingBlocks.Pagination;
using CourtBooking.Application.DTOs;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourtsByOwner
{
    public record GetCourtsByOwnerQuery(
        PaginationRequest PaginationRequest,
        Guid OwnerId,
        Guid? SportId,
        string? CourtType
    ) : IQuery<GetCourtsByOwnerResult>;

    public record GetCourtsByOwnerResult(PaginatedResult<CourtDTO> Courts);
}