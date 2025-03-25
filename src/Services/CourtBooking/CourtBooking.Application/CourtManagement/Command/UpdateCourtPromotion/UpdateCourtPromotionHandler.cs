using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourtBooking.Application.DTOs;
using CourtBooking.Application.Data.Repositories;
using BuildingBlocks.Exceptions;

namespace CourtBooking.Application.CourtManagement.Command.UpdateCourtPromotion
{
    public class UpdateCourtPromotionHandler : IRequestHandler<UpdateCourtPromotionCommand, CourtPromotionDTO>
    {
        private readonly ICourtPromotionRepository _courtPromotionRepository;
        private readonly ICourtRepository _courtRepository;
        private readonly IApplicationDbContext _context;

        public UpdateCourtPromotionHandler(
            ICourtPromotionRepository courtPromotionRepository,
            ICourtRepository courtRepository,
            IApplicationDbContext context)
        {
            _courtPromotionRepository = courtPromotionRepository;
            _courtRepository = courtRepository;
            _context = context;
        }

        public async Task<CourtPromotionDTO> Handle(UpdateCourtPromotionCommand request, CancellationToken cancellationToken)
        {
            var promotion = await _courtPromotionRepository.GetByIdAsync(CourtPromotionId.Of(request.PromotionId), cancellationToken);
            if (promotion == null)
                throw new NotFoundException("Không tìm thấy khuyến mãi.");

            var court = await _courtRepository.GetCourtByIdAsync(promotion.CourtId, cancellationToken);
            var sportCenter = await _context.SportCenters
                .FirstOrDefaultAsync(sc => sc.Id == court.SportCenterId, cancellationToken);
            if (sportCenter == null || sportCenter.OwnerId.Value != request.UserId)
                throw new UnauthorizedAccessException("Bạn không sở hữu sân này.");

            promotion.Update(request.Description, request.DiscountType, request.DiscountValue, request.ValidFrom, request.ValidTo);
            await _courtPromotionRepository.UpdateAsync(promotion, cancellationToken);

            return new CourtPromotionDTO(
                promotion.Id.Value,
                promotion.CourtId.Value,
                promotion.Description,
                promotion.DiscountType,
                promotion.DiscountValue,
                promotion.ValidFrom,
                promotion.ValidTo,
                promotion.CreatedAt,
                promotion.LastModified);
        }
    }
}