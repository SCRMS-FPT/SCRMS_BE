using BuildingBlocks.Pagination;

namespace Identity.Application.Identity.Queries.UserManagement
{
    public record GetUsersQuery(string? SearchTerm, string? Role, PaginationRequest PaginationRequest)
        : IQuery<PaginatedResult<UserDto>>;
}