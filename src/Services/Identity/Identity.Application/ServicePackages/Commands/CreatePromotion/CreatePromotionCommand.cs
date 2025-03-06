using FluentValidation;

namespace Identity.Application.ServicePackages.Commands.CreatePromotion
{
    public record CreatePromotionCommand(
        Guid ServicePackageId,
        string Description,
        string Type,
        decimal Value,
        DateTime ValidFrom,
        DateTime ValidTo
   ) : ICommand<ServicePackagePromotionDto>;

    public class CreatePromotionCommandValidator : AbstractValidator<CreatePromotionCommand>
    {
        public CreatePromotionCommandValidator()
        {
            RuleFor(x => x.ServicePackageId)
             .NotEmpty().WithMessage("Service package ID is required.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Type is required.");

            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Value is required.")
                .GreaterThan(0).WithMessage("Value must greater than 0.");

            RuleFor(x => x.ValidFrom)
                .NotEmpty().WithMessage("Valid from date is required.")
                .LessThan(x => x.ValidTo).WithMessage("Valid from date must be earlier than valid to date.");

            RuleFor(x => x.ValidTo)
                .NotEmpty().WithMessage("Valid to date is required.")
                .GreaterThan(x => x.ValidFrom).WithMessage("Valid to date must be later than valid from date.");

        }
    }
}
