using Microsoft.EntityFrameworkCore;
using Payment.API.Data.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Payment.API.Data.Repositories
{
    public class PendingDepositRepository : IPendingDepositRepository
    {
        private readonly PaymentDbContext _context;

        public PendingDepositRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<PendingDeposit> GetPendingDepositByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.PendingDeposits
                .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
        }

        public async Task<PendingDeposit> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.PendingDeposits
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task AddAsync(PendingDeposit pendingDeposit, CancellationToken cancellationToken = default)
        {
            await _context.PendingDeposits.AddAsync(pendingDeposit, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(PendingDeposit pendingDeposit, CancellationToken cancellationToken = default)
        {
            _context.PendingDeposits.Update(pendingDeposit);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}