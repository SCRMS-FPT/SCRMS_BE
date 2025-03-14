using Coach.API.Data.Models;
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
            var validationErrors = new List<string>();

            // Validate CoachId
            if (promotion.CoachId == Guid.Empty)
            {
                validationErrors.Add("CoachId is required.");
            }

            // Validate DiscountType
            if (string.IsNullOrEmpty(promotion.DiscountType))
            {
                validationErrors.Add("DiscountType is required.");
            }

            // If there are validation errors, throw an exception with all errors
            if (validationErrors.Any())
            {
                throw new ArgumentException(string.Join(" ", validationErrors));
            }

            // If all validations pass, add the promotion
            await _context.CoachPromotions.AddAsync(promotion, cancellationToken);
        }

        public async Task<CoachPromotion?> GetCoachPromotionByIdAsync(Guid promotionId, CancellationToken cancellationToken)
        {
            return await _context.CoachPromotions.FirstOrDefaultAsync(p => p.Id == promotionId, cancellationToken);
        }

        public async Task UpdateCoachPromotionAsync(CoachPromotion promotion, CancellationToken cancellationToken)
        {
            var existingPromotion = await _context.CoachPromotions.FindAsync(promotion.Id);

            if (existingPromotion == null)
            {
                throw new DbUpdateConcurrencyException("The promotion to update does not exist.");
            }

            _context.CoachPromotions.Update(promotion);
            await _context.SaveChangesAsync(cancellationToken); // Save the changes to the database
        }

        public async Task<List<CoachPromotion>> GetCoachPromotionsByCoachIdAsync(Guid coachId, CancellationToken cancellationToken)
        {
            return await _context.CoachPromotions.Where(p => p.CoachId == coachId).ToListAsync(cancellationToken);
        }
    }
}