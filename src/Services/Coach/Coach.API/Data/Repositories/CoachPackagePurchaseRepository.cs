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
            await _context.CoachPackagePurchases.AddAsync(purchase, cancellationToken);
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
            _context.CoachPackagePurchases.Update(purchase);
            await Task.CompletedTask;
        }
    }
}