// DeleteSportCenterCommand.cs
using FluentValidation;

namespace CourtBooking.Application.CourtManagement.Commands.DeleteSportCenter
{
    public record DeleteSportCenterCommand(Guid SportCenterId) : ICommand<DeleteSportCenterResult>;

    public record DeleteSportCenterResult(
        bool Success,
        string Message,
        bool WasDeactivated);

    public class DeleteSportCenterCommandValidator : AbstractValidator<DeleteSportCenterCommand>
    {
        public DeleteSportCenterCommandValidator()
        {
            RuleFor(x => x.SportCenterId)
                .NotEmpty().WithMessage("Sport center ID is required.");
        }
    }
}
