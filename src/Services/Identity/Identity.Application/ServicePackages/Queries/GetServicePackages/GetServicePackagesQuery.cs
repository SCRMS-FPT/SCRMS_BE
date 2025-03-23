using BuildingBlocks.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.ServicePackages.Queries.GetServicePackages
{
    public record GetServicePackagesQuery(
        int PageIndex,
        int PageSize,
        string? Search,
        string? AssociatedRole,
        string? Status,
        string? SortByPrice
    ) : IQuery<PaginatedResult<ServicePackageDto>>;

    public record ServicePackageDto(
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        int DurationDays,
        string AssociatedRole,
        string Status,
        DateTime CreatedAt
    );
}