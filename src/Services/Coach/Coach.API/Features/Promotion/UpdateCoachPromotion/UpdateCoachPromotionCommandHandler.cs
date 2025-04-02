using Coach.API.Data;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Exceptions;

namespace Coach.API.Features.Promotion.UpdateCoachPromotion
{
    public record UpdateCoachPromotionCommand(
        Guid PromotionId,
        Guid? PackageId, // Added PackageId
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
            var promotion = await context.CoachPromotions
                .FirstOrDefaultAsync(cp => cp.Id == command.PromotionId, cancellationToken);

            if (promotion == null)
                throw new NotFoundException("Promotion not found");

            // If PackageId is provided, verify it belongs to the same coach
            if (command.PackageId.HasValue)
            {
                var package = await context.CoachPackages.FindAsync(command.PackageId.Value);
                if (package == null)
                {
                    throw new NotFoundException("Package not found");
                }

                if (package.CoachId != promotion.CoachId)
                {
                    throw new BadRequestException("The package does not belong to this coach");
                }
            }

            promotion.PackageId = command.PackageId; // Set PackageId
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