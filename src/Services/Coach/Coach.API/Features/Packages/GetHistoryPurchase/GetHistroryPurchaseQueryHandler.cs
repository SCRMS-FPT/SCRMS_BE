using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Packages.GetHistoryPurchase
{
    public record GetHistroryPurchaseQuery(Guid UserId, int Page, int RecordPerPage, bool? IsExpiried, bool? IsOutOfUse) : IQuery<List<PurchaseRecord>>;
    public record PurchaseRecord(
     Guid Id,
     Guid CoachPackageId,
     int SessionCount,
     int SessionUsed);

    public class GetHistroryPurchaseQueryValidator : AbstractValidator<GetHistroryPurchaseQuery>
    {
        public GetHistroryPurchaseQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User id is required");
            RuleFor(x => x.Page).NotEmpty().WithMessage("Page number is required").GreaterThan(0).WithMessage("This must be greater than 0.");
            RuleFor(x => x.RecordPerPage).NotEmpty().WithMessage("Record per page is required").GreaterThan(0).WithMessage("This must be greater than 0.");
        }
    }

    public class GetPurchaseQueryHandler(CoachDbContext context)
        : IQueryHandler<GetHistroryPurchaseQuery, List<PurchaseRecord>>
    {
        public async Task<List<PurchaseRecord>> Handle(GetHistroryPurchaseQuery query, CancellationToken cancellationToken)
        {
            var history = await context.CoachPackagePurchases.Include(cpp => cpp.CoachPackage).Where(p => p.UserId == query.UserId).ToListAsync(cancellationToken);

            if (query.IsExpiried != null)
            {
                history = history.Where(p => p.ExpiryDate.CompareTo(DateTime.UtcNow) <= 0).ToList();
            }

            if (query.IsOutOfUse != null)
            {
                history = history.Where(p => p.CoachPackage.SessionCount == p.SessionsUsed).ToList();
            }

            return history.Skip((query.Page - 1) * query.RecordPerPage).Take(query.RecordPerPage)
                .Select(p => new PurchaseRecord(p.Id, p.CoachPackageId, p.CoachPackage.SessionCount, p.SessionsUsed)).ToList();
        }
    }
}