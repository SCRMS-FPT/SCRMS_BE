using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.ServicePackages.Queries.GetServicePackages
{
    public record GetServicePackagesQuery : IQuery<List<ServicePackageDto>>;

    public record ServicePackageDto(
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        int DurationDays,
        string AssociatedRole,
        DateTime CreatedAt
    );
}