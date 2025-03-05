using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.ServicePackages.Commands.DeletePromotion
{
    public record DeletePromotionCommand(Guid PromotionId, Guid CoachId) : ICommand<Unit>;

    public class DeletePromotionCommandValidator : AbstractValidator<DeletePromotionCommand>
    {
        public DeletePromotionCommandValidator()
        {
            RuleFor(x => x.PromotionId).NotEmpty().WithMessage("Promotion id is required");
        }
    }
}
