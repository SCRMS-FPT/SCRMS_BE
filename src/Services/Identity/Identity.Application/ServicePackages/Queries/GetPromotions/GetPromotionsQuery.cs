using BuildingBlocks.Pagination;
using FluentValidation;

namespace Identity.Application.ServicePackages.Queries.GetPromotions
{
    public record GetPromotionsQuery(
        Guid PackageServiceId,
        int PageIndex,
        int PageSize,
        string? Search,
        string? DiscountType
    ) : IQuery<PaginatedResult<ServicePackagePromotionDto>>;

    public class GetPromotionsQueryValidator : AbstractValidator<GetPromotionsQuery>
    {
        public GetPromotionsQueryValidator()
        {
            RuleFor(x => x.PackageServiceId)
                .NotEmpty().WithMessage("Package service id is required");
            RuleFor(x => x.PageIndex)
                .GreaterThanOrEqualTo(0).WithMessage("Page index must be non-negative");
            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0");
        }
    }
}