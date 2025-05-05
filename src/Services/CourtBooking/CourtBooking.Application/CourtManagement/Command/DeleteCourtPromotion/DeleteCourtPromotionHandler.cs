using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourtBooking.Application.DTOs;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.CourtManagement.Commands.DeleteCourtPromotion;
using BuildingBlocks.Exceptions;

namespace CourtBooking.Application.CourtManagement.Command.DeleteCourtPromotion
{
    public class DeleteCourtPromotionHandler : IRequestHandler<DeleteCourtPromotionCommand, DeleteCourtPromotionResult>
    {
        private readonly ICourtPromotionRepository _courtPromotionRepository;
        private readonly ICourtRepository _courtRepository;
        private readonly IApplicationDbContext _context;

        public DeleteCourtPromotionHandler(
            ICourtPromotionRepository courtPromotionRepository,
            ICourtRepository courtRepository,
            IApplicationDbContext context)
        {
            _courtPromotionRepository = courtPromotionRepository;
            _courtRepository = courtRepository;
            _context = context;
        }

        public async Task<DeleteCourtPromotionResult> Handle(DeleteCourtPromotionCommand request, CancellationToken cancellationToken)
        {
            var promotion = await _courtPromotionRepository.GetByIdAsync(CourtPromotionId.Of(request.PromotionId), cancellationToken);
            if (promotion == null)
                throw new NotFoundException("Không tìm thấy khuyến mãi.");

            var court = await _courtRepository.GetCourtByIdAsync(promotion.CourtId, cancellationToken);
            var sportCenter = await _context.SportCenters
                .FirstOrDefaultAsync(sc => sc.Id == court.SportCenterId, cancellationToken);
            if (sportCenter == null || sportCenter.OwnerId.Value != request.UserId)
                throw new UnauthorizedAccessException("Bạn không sở hữu sân này.");

            await _courtPromotionRepository.DeleteAsync(CourtPromotionId.Of(request.PromotionId), cancellationToken);

            // Return the result indicating success
            return new DeleteCourtPromotionResult(true);
        }
    }
}