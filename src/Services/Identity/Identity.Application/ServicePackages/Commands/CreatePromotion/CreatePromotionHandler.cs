using Mapster;

namespace Identity.Application.ServicePackages.Commands.CreatePromotion
{
    public class CreatePromotionHandler :
         ICommandHandler<CreatePromotionCommand, ServicePackagePromotionDto>
    {
        private readonly IApplicationDbContext _context;

        public CreatePromotionHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServicePackagePromotionDto> Handle(
            CreatePromotionCommand request,
            CancellationToken cancellationToken)
        {
            var promotion = new ServicePackagePromotion
            {
                CreatedAt = DateTime.UtcNow,
                Description = request.Description,
                DiscountType = request.Type,
                DiscountValue = request.Value,
                UpdatedAt = DateTime.UtcNow,
                ServicePackageId = request.ServicePackageId,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo,
            };

            _context.ServicePackagePromotions.Add(promotion);
            await _context.SaveChangesAsync(cancellationToken);

            return promotion.Adapt<ServicePackagePromotionDto>();
        }
    }
}
