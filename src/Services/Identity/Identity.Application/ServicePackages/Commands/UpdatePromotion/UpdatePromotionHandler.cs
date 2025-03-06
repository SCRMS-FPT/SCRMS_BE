using Mapster;

namespace Identity.Application.ServicePackages.Commands.UpdatePromotion
{
    public class UpdatePromotionHandler :
        ICommandHandler<UpdatePromotionCommand, ServicePackagePromotionDto>
    {
        private readonly IApplicationDbContext _context;

        public UpdatePromotionHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServicePackagePromotionDto> Handle(
            UpdatePromotionCommand request,
            CancellationToken cancellationToken)
        {
            var promotion = await _context.ServicePackagePromotions.FirstOrDefaultAsync(p => p.Id == request.Id);
            if (promotion == null) { 
                throw new NotFoundException("promotion", request.Id);
            }

            promotion.UpdatedAt = DateTime.UtcNow;
            promotion.Description = request.Description;    
            promotion.DiscountType = request.Type;
            promotion.DiscountValue = request.Value;
            promotion.ValidFrom = request.ValidFrom;
            promotion.ValidTo = request.ValidTo;

            _context.ServicePackagePromotions.Update(promotion);
            await _context.SaveChangesAsync(cancellationToken);

            return promotion.Adapt<ServicePackagePromotionDto>();
        }
    }
}
