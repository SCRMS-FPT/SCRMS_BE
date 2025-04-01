using Coach.API.Data;
using BuildingBlocks.Exceptions;

namespace Coach.API.Features.Promotion.CreateCoachPromotion
{
    public record CreateCoachPromotionCommand(
        Guid CoachId,
        Guid? PackageId, // Added PackageId
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
            // If PackageId is provided, verify it belongs to the same coach
            if (command.PackageId.HasValue)
            {
                var package = await context.CoachPackages.FindAsync(command.PackageId.Value);
                if (package == null)
                {
                    throw new NotFoundException("Package not found");
                }

                if (package.CoachId != command.CoachId)
                {
                    throw new BadRequestException("The package does not belong to this coach");
                }
            }

            var coachPromotion = new CoachPromotion
            {
                Id = Guid.NewGuid(),
                CoachId = command.CoachId,
                PackageId = command.PackageId, // Set the PackageId
                Description = command.Description,
                DiscountType = command.DiscountType,
                DiscountValue = command.DiscountValue,
                ValidFrom = command.ValidFrom,
                ValidTo = command.ValidTo,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context.CoachPromotions.AddAsync(coachPromotion, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return new CreateCoachPromotionResult(coachPromotion.Id);
        }
    }
}