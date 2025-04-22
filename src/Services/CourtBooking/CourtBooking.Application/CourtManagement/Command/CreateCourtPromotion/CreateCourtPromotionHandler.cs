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
    public class CreateCourtPromotionHandler : IRequestHandler<CreateCourtPromotionCommand, CourtPromotionDTO>
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

        public async Task<CourtPromotionDTO> Handle(CreateCourtPromotionCommand request, CancellationToken cancellationToken)
        {
            var court = await _courtRepository.GetCourtByIdAsync(CourtId.Of(request.CourtId), cancellationToken);
            if (court == null)
                throw new NotFoundException("Không tìm thấy sân.");

            var sportCenter = await _context.SportCenters
                .FirstOrDefaultAsync(sc => sc.Id == court.SportCenterId, cancellationToken);

            if (sportCenter == null || sportCenter.OwnerId.Value != request.UserId)
                throw new UnauthorizedAccessException("Bạn không sở hữu sân này.");

            // Validate discount value for percentage type
            if (request.DiscountType == "Percentage" && (request.DiscountValue <= 0 || request.DiscountValue > 100))
            {
                throw new ArgumentException("Giá trị khuyến mãi phần trăm phải nằm trong khoảng từ 1 đến 100.");
            }

            var promotion = CourtPromotion.Create(
                CourtId.Of(request.CourtId),
                request.Description,
                request.DiscountType,
                request.DiscountValue,
                request.ValidFrom,
                request.ValidTo);

            await _courtPromotionRepository.AddAsync(promotion, cancellationToken);

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