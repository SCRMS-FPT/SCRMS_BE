using Microsoft.EntityFrameworkCore;
using Identity.Application.Data;
using BuildingBlocks.Pagination; // Namespace cho PaginatedResult

namespace Identity.Application.ServicePackages.Queries.GetPromotions
{
    public class GetPromotionsHandler : IQueryHandler<GetPromotionsQuery, PaginatedResult<ServicePackagePromotionDto>>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetPromotionsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PaginatedResult<ServicePackagePromotionDto>> Handle(GetPromotionsQuery query, CancellationToken cancellationToken)
        {
            var promotionsQuery = _dbContext.ServicePackagePromotions
                .Where(p => p.ServicePackageId == query.PackageServiceId)
                .AsQueryable();

            // Search theo Description
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                promotionsQuery = promotionsQuery.Where(p => p.Description.Contains(query.Search));
            }

            // Filter theo DiscountType
            if (!string.IsNullOrWhiteSpace(query.DiscountType))
            {
                promotionsQuery = promotionsQuery.Where(p => p.DiscountType == query.DiscountType);
            }

            // Tính tổng số bản ghi
            var totalCount = await promotionsQuery.CountAsync(cancellationToken);

            // Áp dụng phân trang
            var promotions = await promotionsQuery
                .OrderBy(p => p.CreatedAt)
                .Skip(query.PageIndex * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new ServicePackagePromotionDto(
                    p.Id,
                    p.ServicePackageId,
                    p.Description,
                    p.DiscountType,
                    p.DiscountValue,
                    p.ValidFrom,
                    p.ValidTo,
                    p.CreatedAt,
                    p.UpdatedAt
                ))
                .ToListAsync(cancellationToken);

            return new PaginatedResult<ServicePackagePromotionDto>(
                query.PageIndex,
                query.PageSize,
                totalCount,
                promotions
            );
        }
    }
}