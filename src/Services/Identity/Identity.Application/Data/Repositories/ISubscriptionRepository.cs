using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Data.Repositories
{
    public interface ISubscriptionRepository
    {
        Task<ServicePackageSubscription> GetByIdAsync(Guid subscriptionId);

        Task<List<ServicePackageSubscription>> GetByUserIdAsync(Guid userId);

        Task AddAsync(ServicePackageSubscription subscription);

        Task UpdateAsync(ServicePackageSubscription subscription);

        Task DeleteAsync(ServicePackageSubscription subscription);

        Task<bool> ExistsByPackageIdAsync(Guid packageId);
    }
}