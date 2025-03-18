using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Promotion.UpdateCoachPromotion
{
    public record UpdateCoachPromotionCommand(
        Guid PromotionId,
        string Description,
        string DiscountType,
        decimal DiscountValue,
        DateOnly ValidFrom,
        DateOnly ValidTo) : ICommand<Unit>;
    public class UpdateCoachPromotionCommandValidator : AbstractValidator<UpdateCoachPromotionCommand>
    {
        public UpdateCoachPromotionCommandValidator()
        {
            RuleFor(x => x.PromotionId).NotEmpty().WithMessage("Promotion id is required");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required");
            RuleFor(x => x.DiscountType).NotEmpty().WithMessage("Discount type is required");
            RuleFor(x => x.DiscountValue)
                .NotEmpty().WithMessage("Discount value is required").GreaterThan(0).WithMessage("Discount value must greater than 0");

            RuleFor(x => x.ValidFrom)
                .LessThan(x => x.ValidTo)
                .WithMessage("Valid to must be before valid from");
        }
    }
    public class UpdateCoachPromotionCommandHandler(CoachDbContext context)
        : ICommandHandler<UpdateCoachPromotionCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateCoachPromotionCommand command, CancellationToken cancellationToken)
        {
            var promotion = await context.CoachPromotions.FirstOrDefaultAsync(cp => cp.Id == command.PromotionId);
            promotion.Description = command.Description;
            promotion.DiscountType = command.DiscountType;
            promotion.DiscountValue = command.DiscountValue;
            promotion.ValidFrom = command.ValidFrom;
            promotion.ValidTo = command.ValidTo;
            promotion.UpdatedAt = DateTime.UtcNow;
            context.CoachPromotions.Update(promotion);
            await context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
