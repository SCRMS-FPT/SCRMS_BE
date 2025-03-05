using FluentValidation;

namespace Identity.Application.ServicePackages.Queries.GetPromotions
{
    public record GetPromotionsQuery(Guid PackageServiceId) : IQuery<List<ServicePackagePromotionDto>>;

    public class GetPromotionsQueryValidator : AbstractValidator<GetPromotionsQuery>
    {
        public GetPromotionsQueryValidator()
        {
            RuleFor(x => x.PackageServiceId).NotEmpty().WithMessage("Package service id is required");
        }
    }
}
