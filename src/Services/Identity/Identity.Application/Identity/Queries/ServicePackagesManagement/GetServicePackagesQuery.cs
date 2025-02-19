using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Identity.Queries.ServicePackagesManagement
{
    public record GetServicePackagesQuery : IQuery<IEnumerable<ServicePackageDto>>;
}
