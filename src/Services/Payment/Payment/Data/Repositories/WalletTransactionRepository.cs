using Microsoft.EntityFrameworkCore;

namespace Payment.API.Data.Repositories
{
    public class WalletTransactionRepository : IWalletTransactionRepository
    {
        private readonly PaymentDbContext _context;

        public WalletTransactionRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task AddWalletTransactionAsync(WalletTransaction transaction, CancellationToken cancellationToken)
        {
            await _context.WalletTransactions.AddAsync(transaction, cancellationToken);
        }
        public async Task<WalletTransaction?> GetByReferenceCodeAsync(Guid referenceCode)
        {
            return await _context.WalletTransactions
                .FirstOrDefaultAsync(t => t.ReferenceId == referenceCode);
        }
        public async Task<long> GetTransactionCountByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.WalletTransactions
                .Where(t => t.UserId == userId)
                .LongCountAsync(cancellationToken);
        }
        public async Task<List<WalletTransaction>> GetTransactionsByUserIdAsync(Guid userId, int page, int limit, CancellationToken cancellationToken)
        {
            return await _context.WalletTransactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }
        public async Task<WalletTransaction> GetRecentTransactionByUserIdAsync(Guid userId, DateTime sinceTime, CancellationToken cancellationToken = default)
        {
            return await _context.WalletTransactions
                .Where(t => t.UserId == userId && t.CreatedAt >= sinceTime)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}