using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourtBooking.Application.DTOs;
using CourtBooking.Application.Data.Repositories;
using BuildingBlocks.Exceptions;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourtPromotions
{
    public class GetCourtPromotionsHandler : IRequestHandler<GetCourtPromotionsQuery, List<CourtPromotionDto>>
    {
        private readonly ICourtPromotionRepository _courtPromotionRepository;
        private readonly ICourtRepository _courtRepository;
        private readonly IApplicationDbContext _context;

        public GetCourtPromotionsHandler(
            ICourtPromotionRepository courtPromotionRepository,
            ICourtRepository courtRepository,
            IApplicationDbContext context)
        {
            _courtPromotionRepository = courtPromotionRepository;
            _courtRepository = courtRepository;
            _context = context;
        }

        public async Task<List<CourtPromotionDto>> Handle(GetCourtPromotionsQuery request, CancellationToken cancellationToken)
        {
            var court = await _courtRepository.GetCourtByIdAsync(CourtId.Of(request.CourtId), cancellationToken);
            if (court == null)
                throw new NotFoundException("Không tìm thấy sân.");

            // Kiểm tra quyền: CourtOwner hoặc Admin mới được xem danh sách
            if (request.Role != "CourtOwner" && request.Role != "Admin")
            {
                // Người dùng thường vẫn có thể xem, nhưng không cần kiểm tra quyền sở hữu
                var promotions = await _courtPromotionRepository.GetPromotionsByCourtIdAsync(CourtId.Of(request.CourtId), cancellationToken);
                return promotions.Select(p => MapToDto(p)).ToList();
            }

            // Nếu là CourtOwner, kiểm tra xem user có sở hữu sân không
            if (request.Role == "CourtOwner")
            {
                var sportCenter = await _context.SportCenters
                    .FirstOrDefaultAsync(sc => sc.Id.Value == court.SportCenterId.Value, cancellationToken);
                if (sportCenter == null || sportCenter.OwnerId.Value != request.UserId)
                    throw new UnauthorizedAccessException("Bạn không sở hữu sân này.");
            }

            var ownerPromotions = await _courtPromotionRepository.GetPromotionsByCourtIdAsync(CourtId.Of(request.CourtId), cancellationToken);
            return ownerPromotions.Select(p => MapToDto(p)).ToList();
        }

        private CourtPromotionDto MapToDto(CourtPromotion p) => new(
            p.Id.Value,
            p.CourtId.Value,
            p.Description,
            p.DiscountType,
            p.DiscountValue,
            p.ValidFrom,
            p.ValidTo,
            p.CreatedAt,
            p.LastModified);
    }
}