using Coach.API.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Data.Repositories
{
    public class CoachPackagePurchaseRepository : ICoachPackagePurchaseRepository
    {
        private readonly CoachDbContext _context;

        public CoachPackagePurchaseRepository(CoachDbContext context)
        {
            _context = context;
        }

        public async Task AddCoachPackagePurchaseAsync(CoachPackagePurchase purchase, CancellationToken cancellationToken)
        {
            if (purchase.UserId == Guid.Empty)
            {
                throw new ArgumentException("UserId is required", nameof(purchase.UserId));
            }

            if (purchase.CoachPackageId == Guid.Empty)
            {
                throw new ArgumentException("CoachPackageId is required", nameof(purchase.CoachPackageId));
            }

            await _context.CoachPackagePurchases.AddAsync(purchase, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<CoachPackagePurchase?> GetCoachPackagePurchaseByIdAsync(Guid purchaseId, CancellationToken cancellationToken)
        {
            return await _context.CoachPackagePurchases.FirstOrDefaultAsync(p => p.Id == purchaseId, cancellationToken);
        }

        public async Task<List<CoachPackagePurchase>> GetCoachPackagePurchasesByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.CoachPackagePurchases.Where(p => p.UserId == userId).ToListAsync(cancellationToken);
        }

        public async Task UpdateCoachPackagePurchaseAsync(CoachPackagePurchase purchase, CancellationToken cancellationToken)
        {
            var existingPurchase = await _context.CoachPackagePurchases
                .FirstOrDefaultAsync(p => p.Id == purchase.Id, cancellationToken);

            if (existingPurchase == null)
            {
                throw new DbUpdateConcurrencyException("The purchase does not exist.");
            }

            // If the purchase exists, update it
            _context.CoachPackagePurchases.Update(purchase);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}