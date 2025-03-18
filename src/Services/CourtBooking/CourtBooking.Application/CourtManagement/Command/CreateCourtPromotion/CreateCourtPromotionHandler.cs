using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourtBooking.Application.DTOs;
using CourtBooking.Application.Data.Repositories;
using BuildingBlocks.Exceptions;

namespace CourtBooking.Application.CourtManagement.Command.CreateCourtPromotion
{
    public class CreateCourtPromotionHandler : IRequestHandler<CreateCourtPromotionCommand, CourtPromotionDto>
    {
        private readonly ICourtPromotionRepository _courtPromotionRepository;
        private readonly ICourtRepository _courtRepository;
        private readonly IApplicationDbContext _context;

        public CreateCourtPromotionHandler(
            ICourtPromotionRepository courtPromotionRepository,
            ICourtRepository courtRepository,
            IApplicationDbContext context)
        {
            _courtPromotionRepository = courtPromotionRepository;
            _courtRepository = courtRepository;
            _context = context;
        }

        public async Task<CourtPromotionDto> Handle(CreateCourtPromotionCommand request, CancellationToken cancellationToken)
        {
            var court = await _courtRepository.GetCourtByIdAsync(CourtId.Of(request.CourtId), cancellationToken);
            if (court == null)
                throw new NotFoundException("Không tìm thấy sân.");

            var sportCenter = await _context.SportCenters
                .FirstOrDefaultAsync(sc => sc.Id == court.SportCenterId, cancellationToken);

            if (sportCenter == null || sportCenter.OwnerId.Value != request.UserId)
                throw new UnauthorizedAccessException("Bạn không sở hữu sân này.");

            var promotion = CourtPromotion.Create(
                CourtId.Of(request.CourtId),
                request.Description,
                request.DiscountType,
                request.DiscountValue,
                request.ValidFrom,
                request.ValidTo);

            await _courtPromotionRepository.AddAsync(promotion, cancellationToken);

            return new CourtPromotionDto(
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