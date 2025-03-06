using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Data.Repositories
{
    public interface ISubscriptionRepository
    {
        Task<ServicePackageSubscription> GetSubscriptionByIdAsync(Guid subscriptionId);

        Task<List<ServicePackageSubscription>> GetSubscriptionByUserIdAsync(Guid userId);

        Task AddSubscriptionAsync(ServicePackageSubscription subscription);

        Task UpdateSubscriptionAsync(ServicePackageSubscription subscription);

        Task DeleteSubscriptionAsync(ServicePackageSubscription subscription);

        Task<bool> ExistsSubscriptionByPackageIdAsync(Guid packageId);
    }
}