using Coach.API.Data;

namespace Coach.API.Promotion.CreateCoachPromotion
{
    public record CreateCoachPromotionCommand(
        Guid CoachId,
        string Description,
        string DiscountType,
        decimal DiscountValue,
        DateOnly ValidFrom,
        DateOnly ValidTo) : ICommand<CreateCoachPromotionResult>;

    public record CreateCoachPromotionResult(
        Guid Id);

    public class CreateCoachPromotionCommandValidator : AbstractValidator<CreateCoachPromotionCommand>
    {
        public CreateCoachPromotionCommandValidator()
        {
            RuleFor(x => x.CoachId).NotEmpty().WithMessage("Coach id is required");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required");
            RuleFor(x => x.DiscountType).NotEmpty().WithMessage("Discount type is required");
            RuleFor(x => x.DiscountValue)
                .NotEmpty().WithMessage("Discount value is required").GreaterThan(0).WithMessage("Discount value must greater than 0");

            RuleFor(x => x.ValidFrom)
                .LessThan(x => x.ValidTo)
                .WithMessage("Valid to must be before valid from");
        }
    }
    public class CreateCoachPromotionCommandHandler(CoachDbContext context)
        : ICommandHandler<CreateCoachPromotionCommand, CreateCoachPromotionResult>
    {
        public async Task<CreateCoachPromotionResult> Handle(CreateCoachPromotionCommand command, CancellationToken cancellationToken)
        {
            var coachPromotion = new CoachPromotion
            {
                ValidTo = command.ValidTo,
                CoachId = command.CoachId,
                Description = command.Description,
                DiscountType = command.DiscountType,
                DiscountValue = command.DiscountValue,
                ValidFrom = command.ValidFrom,
            };
            await context.CoachPromotions.AddAsync(coachPromotion, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return new CreateCoachPromotionResult(coachPromotion.Id);
        }
    }
}
