using Microsoft.EntityFrameworkCore;
using Payment.API.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Payment.API.Data.Repositories
{
    public class WithdrawalRequestRepository : IWithdrawalRequestRepository
    {
        private readonly PaymentDbContext _context;

        public WithdrawalRequestRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<WithdrawalRequest> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.WithdrawalRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }

        public async Task<List<WithdrawalRequest>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.WithdrawalRequests
                .AsNoTracking()
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<WithdrawalRequest>> GetPendingRequestsAsync(int page, int limit, CancellationToken cancellationToken)
        {
            return await _context.WithdrawalRequests
                .AsNoTracking()
                .Where(w => w.Status == "Pending")
                .OrderBy(w => w.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(WithdrawalRequest request, CancellationToken cancellationToken)
        {
            await _context.WithdrawalRequests.AddAsync(request, cancellationToken);
        }

        public async Task UpdateAsync(WithdrawalRequest request, CancellationToken cancellationToken)
        {
            _context.WithdrawalRequests.Update(request);
            await Task.CompletedTask;
        }
    }
}