using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Data
{
    public interface IApplicationDbContext
    {
        DbSet<ServicePackage> ServicePackages { get; }
        DbSet<ServicePackageSubscription> Subscriptions { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
