using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.ServicePackages.Queries.GetServicePackages
{
    public class GetServicePackagesHandler : IQueryHandler<GetServicePackagesQuery, List<ServicePackageDto>>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetServicePackagesHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ServicePackageDto>> Handle(GetServicePackagesQuery query, CancellationToken cancellationToken)
        {
            return await _dbContext.ServicePackagePromotions
                .Select(p => new ServicePackageDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.DurationDays,
                    p.AssociatedRole,
                    p.CreatedAt
                ))
                .ToListAsync(cancellationToken);
        }
    }
}