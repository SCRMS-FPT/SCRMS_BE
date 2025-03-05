using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Data.Repositories
{
    public interface IServicePackageRepository
    {
        Task<ServicePackage> GetByIdAsync(Guid packageId);

        Task<List<ServicePackage>> GetAllAsync();

        Task AddAsync(ServicePackage package);

        Task UpdateAsync(ServicePackage package);

        Task DeleteAsync(ServicePackage package);
    }
}