namespace Identity.Application.Data
{
    public interface IApplicationDbContext
    {
        DbSet<ServicePackage> ServicePackages { get; }
        DbSet<ServicePackageSubscription> Subscriptions { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}