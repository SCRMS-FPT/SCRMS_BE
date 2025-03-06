using Identity.Application.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Test.Fakes
{
    public class FakeServicePackageRepository : IServicePackageRepository
    {
        private readonly IdentityDbContext _context;

        public FakeServicePackageRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<ServicePackage> GetServicePackageByIdAsync(Guid packageId)
        {
            return await _context.ServicePackages.FindAsync(packageId);
        }

        public async Task<List<ServicePackage>> GetAllServicePackageAsync()
        {
            return await _context.ServicePackages.ToListAsync();
        }

        public async Task AddServicePackageAsync(ServicePackage package)
        {
            _context.ServicePackages.Add(package);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        public async Task UpdateServicePackageAsync(ServicePackage package)
        {
            _context.ServicePackages.Update(package);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        public async Task DeleteServicePackageAsync(ServicePackage package)
        {
            _context.ServicePackages.Remove(package);
            await _context.SaveChangesAsync(CancellationToken.None);
        }
    }
}