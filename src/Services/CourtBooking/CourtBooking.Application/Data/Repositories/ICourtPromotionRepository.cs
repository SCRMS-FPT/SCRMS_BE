using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CourtBooking.Domain.Models;

namespace CourtBooking.Application.Data.Repositories
{
    public interface ICourtPromotionRepository
    {
        Task<List<CourtPromotion>> GetPromotionsByCourtIdAsync(CourtId courtId, CancellationToken cancellationToken);

        Task AddAsync(CourtPromotion promotion, CancellationToken cancellationToken);

        Task<CourtPromotion> GetByIdAsync(CourtPromotionId promotionId, CancellationToken cancellationToken);

        Task UpdateAsync(CourtPromotion promotion, CancellationToken cancellationToken);

        Task DeleteAsync(CourtPromotionId promotionId, CancellationToken cancellationToken);

        /// <summary>
        /// Lấy tất cả khuyến mãi hiện hành cho một sân trong khoảng thời gian
        /// </summary>
        Task<IEnumerable<CourtPromotion>> GetValidPromotionsForCourtAsync(CourtId courtId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
    }
}