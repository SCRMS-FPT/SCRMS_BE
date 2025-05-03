using Microsoft.EntityFrameworkCore;
using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Mapster;
using BuildingBlocks.Pagination;

namespace Identity.Application.ServicePackages.Queries.GetServicePackages
{
    public class GetServicePackagesHandler : IQueryHandler<GetServicePackagesQuery, PaginatedResult<ServicePackageDto>>
    {
        private readonly IServicePackageRepository _packageRepository;

        public GetServicePackagesHandler(IServicePackageRepository packageRepository)
        {
            _packageRepository = packageRepository;
        }

        public async Task<PaginatedResult<ServicePackageDto>> Handle(GetServicePackagesQuery query, CancellationToken cancellationToken)
        {
            var packagesQuery = _packageRepository.GetServicePackagesQueryable();

            // Áp dụng search theo tên
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                packagesQuery = packagesQuery.Where(p => p.Name.ToLower().Contains(query.Search.ToLower()));
            }

            // Áp dụng filter theo AssociatedRole
            if (!string.IsNullOrWhiteSpace(query.AssociatedRole))
            {
                packagesQuery = packagesQuery.Where(p => p.AssociatedRole == query.AssociatedRole);
            }

            // Áp dụng filter theo Status
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                packagesQuery = packagesQuery.Where(p => p.Status == query.Status);
            }

            // Áp dụng sort theo Price
            if (!string.IsNullOrWhiteSpace(query.SortByPrice))
            {
                if (query.SortByPrice.ToLower() == "asc")
                {
                    packagesQuery = packagesQuery.OrderBy(p => p.Price);
                }
                else if (query.SortByPrice.ToLower() == "desc")
                {
                    packagesQuery = packagesQuery.OrderByDescending(p => p.Price);
                }
            }

            // Tính tổng số bản ghi trước khi phân trang
            var totalCount = await packagesQuery.CountAsync(cancellationToken);

            // Áp dụng phân trang
            var packages = await packagesQuery
                .Skip(query.PageIndex * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            var packageDtos = packages.Select(p => p.Adapt<ServicePackageDto>()).ToList();

            return new PaginatedResult<ServicePackageDto>(
                query.PageIndex,
                query.PageSize,
                totalCount,
                packageDtos
            );
        }
    }
}