using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CourtBooking.Infrastructure.Data.Repositories
{
    public class CourtPromotionRepository : ICourtPromotionRepository
    {
        private readonly ApplicationDbContext _context;

        public CourtPromotionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CourtPromotion>> GetPromotionsByCourtIdAsync(CourtId courtId, CancellationToken cancellationToken)
        {
            return await _context.CourtPromotions
                .Where(p => p.CourtId == courtId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(CourtPromotion promotion, CancellationToken cancellationToken)
        {
            _context.CourtPromotions.Add(promotion);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<CourtPromotion> GetByIdAsync(CourtPromotionId promotionId, CancellationToken cancellationToken)
        {
            return await _context.CourtPromotions
                .FirstOrDefaultAsync(p => p.Id == promotionId, cancellationToken);
        }

        public async Task UpdateAsync(CourtPromotion promotion, CancellationToken cancellationToken)
        {
            _context.CourtPromotions.Update(promotion);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(CourtPromotionId promotionId, CancellationToken cancellationToken)
        {
            var promotion = await GetByIdAsync(promotionId, cancellationToken);
            if (promotion != null)
            {
                _context.CourtPromotions.Remove(promotion);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<CourtPromotion>> GetValidPromotionsForCourtAsync(CourtId courtId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            return await _context.CourtPromotions
                .Where(cp => cp.CourtId == courtId &&
                           cp.ValidFrom <= endDate &&
                           cp.ValidTo >= startDate)
                .ToListAsync(cancellationToken);
        }
    }
}