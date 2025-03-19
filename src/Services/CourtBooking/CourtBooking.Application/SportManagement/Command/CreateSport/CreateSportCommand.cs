using FluentValidation;
using MediatR;

namespace CourtBooking.Application.SportManagement.Commands.CreateSport;
public record CreateSportCommand(string Name, string Description, string Icon) : IRequest<CreateSportResult>;

public record CreateSportResult(Guid Id);

public class CreateSportCommandValidator : AbstractValidator<CreateSportCommand>
{
    public CreateSportCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên môn thể thao không được để trống")
            .MaximumLength(100).WithMessage("Tên môn thể thao không được vượt quá 100 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự");

        RuleFor(x => x.Icon)
            .NotEmpty().WithMessage("Icon không được để trống")
            .MaximumLength(200).WithMessage("Đường dẫn icon không được vượt quá 200 ký tự");
    }
}