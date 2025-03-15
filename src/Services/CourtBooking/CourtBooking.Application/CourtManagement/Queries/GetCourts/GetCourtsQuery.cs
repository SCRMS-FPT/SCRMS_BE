using BuildingBlocks.Pagination;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourts
{
    public record GetCourtsQuery(PaginationRequest PaginationRequest, Guid? sportCenterId, Guid? sportId, string? courtType) : IQuery<GetCourtsResult>;

    public record GetCourtsResult(PaginatedResult<CourtDTO> Courts);
}