using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Packages.GetHistoryPurchase
{
    public record GetHistroryPurchaseQuery(
        Guid UserId,
        int Page,
        int RecordPerPage,
        bool? IsExpiried,
        bool? IsOutOfUse,
        Guid? CoachId) : IQuery<List<PurchaseRecord>>;

    public record PurchaseRecord(
        Guid Id,
        Guid CoachPackageId,
        string PackageName,
        int SessionCount,
        int SessionUsed,
        decimal Price,
        Guid CoachId,
        string CoachName,
        DateTime PurchaseDate,
        DateTime ExpiryDate);

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
            var purchasesQuery = context.CoachPackagePurchases
                .Include(cpp => cpp.CoachPackage)
                .ThenInclude(cp => cp.Coach)
                .Where(p => p.UserId == query.UserId);

            // Lọc theo CoachId nếu được chỉ định
            if (query.CoachId.HasValue)
            {
                purchasesQuery = purchasesQuery.Where(p => p.CoachPackage.CoachId == query.CoachId.Value);
            }

            // Lọc theo trạng thái hết hạn
            if (query.IsExpiried.HasValue)
            {
                if (query.IsExpiried.Value)
                {
                    // Lọc các gói ĐÃ hết hạn
                    purchasesQuery = purchasesQuery.Where(p => p.ExpiryDate < DateTime.UtcNow);
                }
                else
                {
                    // Lọc các gói CHƯA hết hạn
                    purchasesQuery = purchasesQuery.Where(p => p.ExpiryDate >= DateTime.UtcNow);
                }
            }

            // Lọc theo trạng thái sử dụng
            if (query.IsOutOfUse.HasValue)
            {
                if (query.IsOutOfUse.Value)
                {
                    // Lọc các gói ĐÃ dùng hết buổi
                    purchasesQuery = purchasesQuery.Where(p => p.SessionsUsed >= p.CoachPackage.SessionCount);
                }
                else
                {
                    // Lọc các gói CÒN buổi sử dụng
                    purchasesQuery = purchasesQuery.Where(p => p.SessionsUsed < p.CoachPackage.SessionCount);
                }
            }

            // Phân trang kết quả
            var history = await purchasesQuery
                .OrderByDescending(p => p.PurchaseDate)
                .Skip((query.Page - 1) * query.RecordPerPage)
                .Take(query.RecordPerPage)
                .ToListAsync(cancellationToken);

            // Map kết quả sang DTO
            return history.Select(p => new PurchaseRecord(
                p.Id,
                p.CoachPackageId,
                p.CoachPackage.Name,
                p.CoachPackage.SessionCount,
                p.SessionsUsed,
                p.CoachPackage.Price,
                p.CoachPackage.CoachId,
                p.CoachPackage.Coach.FullName,
                p.PurchaseDate,
                p.ExpiryDate
            )).ToList();
        }
    }
}