using CourtBooking.Application.DTOs;
using FluentValidation;

namespace CourtBooking.Application.CourtManagement.Command.CreateCourt;

public record CreateCourtCommand(CourtCreateDTO Court) : ICommand<CreateCourtResult>;

public record CreateCourtResult(Guid Id);

public class CreateCourtCommandValidator : AbstractValidator<CreateCourtCommand>
{
    public CreateCourtCommandValidator()
    {
        RuleFor(x => x.Court).NotNull();
    }
}