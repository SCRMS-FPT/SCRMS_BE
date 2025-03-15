using FluentValidation;

namespace CourtBooking.Application.CourtManagement.Commands.DeleteCourtPromotion;

public record DeleteCourtPromotionCommand(Guid PromotionId, Guid UserId) : IRequest<DeleteCourtPromotionResult>;

public record DeleteCourtPromotionResult(bool Success);

public class DeleteCourtPromotionCommandValidator : AbstractValidator<DeleteCourtPromotionCommand>
{
    public DeleteCourtPromotionCommandValidator()
    {
        RuleFor(x => x.PromotionId).NotEmpty();
    }
}