using FluentValidation;
using MediatR;

namespace CourtBooking.Application.SportManagement.Commands.DeleteSport;

public record DeleteSportCommand(Guid SportId) : IRequest<DeleteSportResult>;

public record DeleteSportResult(bool IsSuccess, string Message);

public class DeleteSportCommandValidator : AbstractValidator<DeleteSportCommand>
{
    public DeleteSportCommandValidator()
    {
        RuleFor(x => x.SportId)
            .NotEqual(Guid.Empty).WithMessage("ID môn thể thao không được để trống");
    }
}