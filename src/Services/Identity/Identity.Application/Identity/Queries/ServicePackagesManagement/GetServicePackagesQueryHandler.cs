using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Identity.Queries.ServicePackagesManagement
{
    public class GetServicePackagesQueryHandler(
        IApplicationDbContext context)
        : IQueryHandler<GetServicePackagesQuery, IEnumerable<ServicePackageDto>>
    {
        public async Task<IEnumerable<ServicePackageDto>> Handle(
            GetServicePackagesQuery request,
            CancellationToken cancellationToken)
        {
            return await context.ServicePackages
                .AsNoTracking()
                .ProjectToType<ServicePackageDto>()
                .ToListAsync(cancellationToken);
        }
    }
}
