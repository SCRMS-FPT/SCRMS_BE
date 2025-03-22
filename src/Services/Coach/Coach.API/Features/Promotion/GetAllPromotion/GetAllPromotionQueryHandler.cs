using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Promotion.GetAllPromotion
{
    public record GetAllPromotionQuery(Guid CoachId, int Page, int RecordPerPage) : IQuery<List<PromotionRecord>>;
    public record PromotionRecord(Guid Id, string Description, string DiscountType, decimal DiscountValue, DateOnly ValidFrom,
        DateOnly ValidTo, DateTime CreatedAt, DateTime UpdatedAt);
    public class GetAllPromotionQueryValidator : AbstractValidator<GetAllPromotionQuery>
    {
        public GetAllPromotionQueryValidator()
        {
            RuleFor(x => x.CoachId).NotEmpty().WithMessage("Coach id is required");
            RuleFor(x => x.Page).NotEmpty().WithMessage("Page number is required");
            RuleFor(x => x.RecordPerPage).NotEmpty().WithMessage("Record per page is required");
        }
    }
    public class GetAllPromotionQueryHandler(CoachDbContext context)
        : IQueryHandler<GetAllPromotionQuery, List<PromotionRecord>>
    {
        public async Task<List<PromotionRecord>> Handle(GetAllPromotionQuery query, CancellationToken cancellationToken)
        {
            var list = await context.CoachPromotions.Where(p => p.CoachId == query.CoachId).ToListAsync(cancellationToken);
            return list.Skip((query.Page - 1) * query.RecordPerPage).Take(query.RecordPerPage)
                .Select(p => new PromotionRecord(p.Id, p.Description, p.DiscountType, p.DiscountValue, p.ValidFrom, p.ValidTo, p.CreatedAt, p.UpdatedAt))
                .ToList();
        }
    }
}
