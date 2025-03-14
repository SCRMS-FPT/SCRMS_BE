using Coach.API.Data.Models;

namespace Coach.API.Data.Repositories
{
    public interface ICoachPackagePurchaseRepository
    {
        Task AddCoachPackagePurchaseAsync(CoachPackagePurchase purchase, CancellationToken cancellationToken);

        Task<CoachPackagePurchase?> GetCoachPackagePurchaseByIdAsync(Guid purchaseId, CancellationToken cancellationToken);

        Task<List<CoachPackagePurchase>> GetCoachPackagePurchasesByUserIdAsync(Guid userId, CancellationToken cancellationToken);

        Task UpdateCoachPackagePurchaseAsync(CoachPackagePurchase purchase, CancellationToken cancellationToken);
    }
}