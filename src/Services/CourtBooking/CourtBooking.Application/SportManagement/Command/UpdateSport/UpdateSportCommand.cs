using FluentValidation;
using MediatR;

namespace CourtBooking.Application.SportManagement.Commands.UpdateSport;

public record UpdateSportCommand(Guid Id, string Name, string Description, string Icon) : IRequest<UpdateSportResult>;

public record UpdateSportResult(bool IsSuccess);

public class UpdateSportCommandValidator : AbstractValidator<UpdateSportCommand>
{
    public UpdateSportCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty).WithMessage("ID môn thể thao không được để trống");

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