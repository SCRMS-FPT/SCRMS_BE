using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Data.Repositories
{
    public interface IServicePackageRepository
    {
        Task<ServicePackage> GetServicePackageByIdAsync(Guid packageId);

        Task<List<ServicePackage>> GetAllServicePackageAsync();

        Task AddServicePackageAsync(ServicePackage package);

        Task UpdateServicePackageAsync(ServicePackage package);

        Task DeleteServicePackageAsync(ServicePackage package);
    }
}