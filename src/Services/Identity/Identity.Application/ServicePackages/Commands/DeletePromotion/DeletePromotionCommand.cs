using FluentValidation;

namespace Identity.Application.ServicePackages.Commands.DeletePromotion
{
    public record DeletePromotionCommand(Guid PromotionId) : ICommand<Unit>;

    public class DeletePromotionCommandValidator : AbstractValidator<DeletePromotionCommand>
    {
        public DeletePromotionCommandValidator()
        {
            RuleFor(x => x.PromotionId).NotEmpty().WithMessage("Promotion id is required");
        }
    }
}
