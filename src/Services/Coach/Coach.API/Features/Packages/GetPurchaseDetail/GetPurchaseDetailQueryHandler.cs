using BuildingBlocks.Exceptions;
using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Packages.GetPurchaseDetail
{
    public record GetPurchaseDetailQuery(Guid PurchaseId, Guid UserId) : IQuery<PurchaseDetail>;
    public record PurchaseDetail(
     Guid Id,
     Guid CoachPackageId,
     DateTime PurchaseDate,
     DateTime ExpiryDate,
     int SessionCount,
     int SessionUsed);
    public class GetHistroryPurchaseQueryValidator : AbstractValidator<GetPurchaseDetailQuery>
    {
        public GetHistroryPurchaseQueryValidator()
        {
            RuleFor(x => x.PurchaseId).NotEmpty().WithMessage("Purchase id is required");
        }
    }
    public class GetPurchaseDetailQueryHandler(CoachDbContext context)
        : IQueryHandler<GetPurchaseDetailQuery, PurchaseDetail>
    {
        public async Task<PurchaseDetail> Handle(GetPurchaseDetailQuery query, CancellationToken cancellationToken)
        {
            var purchase = await context.CoachPackagePurchases.Include(cpp => cpp.CoachPackage).FirstOrDefaultAsync(cpp => cpp.Id == query.PurchaseId);
            if (purchase == null)
            {
                throw new NotFoundException("CoachPackagePurchase", query.PurchaseId);
            }

            if (purchase.UserId != query.UserId)
            {
                throw new BadRequestException("You don't own this purchase");
            }

            return new PurchaseDetail(purchase.Id, purchase.CoachPackageId, purchase.PurchaseDate, purchase.ExpiryDate, purchase.CoachPackage.SessionCount, purchase.SessionsUsed);
        }
    }
}
