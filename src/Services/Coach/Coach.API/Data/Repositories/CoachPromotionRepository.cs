using Microsoft.EntityFrameworkCore;

namespace Coach.API.Data.Repositories
{
    public class CoachPromotionRepository : ICoachPromotionRepository
    {
        private readonly CoachDbContext _context;

        public CoachPromotionRepository(CoachDbContext context)
        {
            _context = context;
        }

        public async Task AddCoachPromotionAsync(CoachPromotion promotion, CancellationToken cancellationToken)
        {
            await _context.CoachPromotions.AddAsync(promotion, cancellationToken);
        }

        public async Task<CoachPromotion?> GetCoachPromotionByIdAsync(Guid promotionId, CancellationToken cancellationToken)
        {
            return await _context.CoachPromotions.FirstOrDefaultAsync(p => p.Id == promotionId, cancellationToken);
        }

        public async Task UpdateCoachPromotionAsync(CoachPromotion promotion, CancellationToken cancellationToken)
        {
            _context.CoachPromotions.Update(promotion);
            await Task.CompletedTask;
        }

        public async Task<List<CoachPromotion>> GetCoachPromotionsByCoachIdAsync(Guid coachId, CancellationToken cancellationToken)
        {
            return await _context.CoachPromotions.Where(p => p.CoachId == coachId).ToListAsync(cancellationToken);
        }
    }
}