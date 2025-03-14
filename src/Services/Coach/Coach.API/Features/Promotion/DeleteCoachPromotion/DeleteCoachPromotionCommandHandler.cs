using BuildingBlocks.Exceptions;
using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Promotion.DeleteCoachPromotion
{
    public record DeleteCoachPromotionCommand(Guid CoachPromotionId) : ICommand<Unit>;
    public class DeleteCoachPromotionCommandValidator : AbstractValidator<DeleteCoachPromotionCommand>
    {
        public DeleteCoachPromotionCommandValidator()
        {
            RuleFor(x => x.CoachPromotionId).NotEmpty().WithMessage("Coach promotion id is required");
        }
    }
    public class DeleteCoachPromotionCommandHandler(CoachDbContext context)
        : ICommandHandler<DeleteCoachPromotionCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteCoachPromotionCommand command, CancellationToken cancellationToken)
        {
            var coachPromotion = await context.CoachPromotions.FirstOrDefaultAsync(cp => cp.Id == command.CoachPromotionId);
            if (coachPromotion == null)
            {
                throw new NotFoundException("CoachPromotion", command.CoachPromotionId);
            }
            // TODO: Check again 
            context.CoachPromotions.Remove(coachPromotion);
            await context.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
