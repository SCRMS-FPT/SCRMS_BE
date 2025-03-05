using Identity.Application.ServicePackages.Commands.CreatePromotion;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.ServicePackages.Commands.DeletePromotion
{
    public class DeletePromotionHandler :
         ICommandHandler<DeletePromotionCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public DeletePromotionHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(
            DeletePromotionCommand request,
            CancellationToken cancellationToken)
        {
            var promotion = await _context.ServicePackagePromotions.FirstOrDefaultAsync(p => p.Id == request.PromotionId);
            if (promotion == null)
            {
                throw new NotFoundException("promotion", request.PromotionId);
            }
            _context.ServicePackagePromotions.Remove(promotion);

            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
