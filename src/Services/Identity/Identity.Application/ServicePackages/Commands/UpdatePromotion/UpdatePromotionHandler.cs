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

            // TODO: Must check the authority of promotion

            promotion.UpdatedAt = DateTime.UtcNow;
            promotion.Description = request.Description;    
            promotion.DiscountType = request.Type;
            promotion.DiscountValue = request.Value;
            promotion.ValidFrom = request.ValidFrom;
            promotion.ValidTo = request.ValidTo;
            //TODO: A little confuse in here, should we also change the id of the package ?
            promotion.ServicePackageId = request.Id;    

            _context.ServicePackagePromotions.Update(promotion);
            await _context.SaveChangesAsync(cancellationToken);

            return promotion.Adapt<ServicePackagePromotionDto>();
        }
    }
}
