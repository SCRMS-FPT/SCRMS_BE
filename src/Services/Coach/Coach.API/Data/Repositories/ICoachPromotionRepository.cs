using Coach.API.Data.Models;

namespace Coach.API.Data.Repositories
{
    public interface ICoachPromotionRepository
    {
        Task AddCoachPromotionAsync(CoachPromotion promotion, CancellationToken cancellationToken);

        Task<CoachPromotion?> GetCoachPromotionByIdAsync(Guid promotionId, CancellationToken cancellationToken);

        Task UpdateCoachPromotionAsync(CoachPromotion promotion, CancellationToken cancellationToken);

        Task<List<CoachPromotion>> GetCoachPromotionsByCoachIdAsync(Guid coachId, CancellationToken cancellationToken);
    }
}