using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.CourtManagement.Command.DeleteCourt
{
    public record DeleteCourtCommand(Guid CourtId)
    : ICommand<DeleteCourtResult>;

    public record DeleteCourtResult(bool IsSuccess);

    public class DeleteCourtCommandValidator : AbstractValidator<DeleteCourtCommand>
    {
        public DeleteCourtCommandValidator()
        {
            RuleFor(x => x.CourtId).NotEmpty().WithMessage("CourtId is required");
        }
    }
}
