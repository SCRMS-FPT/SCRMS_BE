namespace Identity.Application.ServicePackages.Queries.GetPromotions
{
    public class GetPromotionsHandler : IQueryHandler<GetPromotionsQuery, List<ServicePackagePromotionDto>>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetPromotionsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ServicePackagePromotionDto>> Handle(GetPromotionsQuery query, CancellationToken cancellationToken)
        {
            return await _dbContext.ServicePackagePromotions
                .Where(p => p.ServicePackageId == query.PackageServiceId)
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
        }
    }
}
