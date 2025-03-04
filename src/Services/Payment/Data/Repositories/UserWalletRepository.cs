using Microsoft.EntityFrameworkCore;
using Payment.API.Data.Models;

namespace Payment.API.Data.Repositories
{
    public class UserWalletRepository : IUserWalletRepository
    {
        private readonly PaymentDbContext _context;

        public UserWalletRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<UserWallet?> GetUserWalletByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.UserWallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
        }

        public async Task AddUserWalletAsync(UserWallet wallet, CancellationToken cancellationToken)
        {
            await _context.UserWallets.AddAsync(wallet, cancellationToken);
        }

        public Task UpdateUserWalletAsync(UserWallet wallet, CancellationToken cancellationToken)
        {
            _context.UserWallets.Update(wallet);
            return Task.CompletedTask;
        }
    }
}