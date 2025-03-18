using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Identity.Application.Data
{
    public interface IApplicationDbContext
    {
        DatabaseFacade Database { get; }
        DbSet<ServicePackage> ServicePackages { get; }
        DbSet<ServicePackageSubscription> Subscriptions { get; }
        DbSet<ServicePackagePromotion> ServicePackagePromotions { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}