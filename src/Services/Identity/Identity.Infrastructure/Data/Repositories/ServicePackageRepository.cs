using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Identity.Domain.Models;

namespace Identity.Infrastructure.Data.Repositories
{
    public class ServicePackageRepository : IServicePackageRepository
    {
        private readonly IApplicationDbContext _dbContext;

        public ServicePackageRepository(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServicePackage> GetByIdAsync(Guid packageId)
        {
            return await _dbContext.ServicePackages.FindAsync(packageId);
        }

        public async Task<List<ServicePackage>> GetAllAsync()
        {
            return await _dbContext.ServicePackages.ToListAsync();
        }

        public async Task AddAsync(ServicePackage package)
        {
            _dbContext.ServicePackages.Add(package);
            await _dbContext.SaveChangesAsync(CancellationToken.None);
        }

        public async Task UpdateAsync(ServicePackage package)
        {
            _dbContext.ServicePackages.Update(package);
            await _dbContext.SaveChangesAsync(CancellationToken.None);
        }

        public async Task DeleteAsync(ServicePackage package)
        {
            _dbContext.ServicePackages.Remove(package);
            await _dbContext.SaveChangesAsync(CancellationToken.None);
        }
    }
}