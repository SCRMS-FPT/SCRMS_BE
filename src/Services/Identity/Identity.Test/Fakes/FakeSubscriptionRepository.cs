using Identity.Application.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Test.Fakes
{
    public class FakeSubscriptionRepository : ISubscriptionRepository
    {
        private readonly IdentityDbContext _context;

        public FakeSubscriptionRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<ServicePackageSubscription> GetSubscriptionByIdAsync(Guid subscriptionId)
        {
            return await _context.Subscriptions.FindAsync(subscriptionId);
        }

        public async Task<List<ServicePackageSubscription>> GetSubscriptionByUserIdAsync(Guid userId)
        {
            return await _context.Subscriptions.Where(s => s.UserId == userId).ToListAsync();
        }

        public async Task AddSubscriptionAsync(ServicePackageSubscription subscription)
        {
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        public async Task UpdateSubscriptionAsync(ServicePackageSubscription subscription)
        {
            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        public async Task DeleteSubscriptionAsync(ServicePackageSubscription subscription)
        {
            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        public async Task<bool> ExistsSubscriptionByPackageIdAsync(Guid packageId)
        {
            return await _context.Subscriptions.AnyAsync(s => s.PackageId == packageId);
        }
    }
}