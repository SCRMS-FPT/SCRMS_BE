using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Identity.Application.Identity.Commands.UpdateProfile
{
    public record UpdateProfileCommand(
        Guid UserId,
        string FirstName,
        string LastName,
        string Phone,
        DateTime BirthDate,
        string Gender,
        string? SelfIntroduction = null,
        IFormFile? NewAvatarFile = null,
        List<IFormFile>? NewImageFiles = null,
        List<string>? ExistingImageUrls = null,
        List<string>? ImagesToDelete = null
    ) : ICommand<UserDto>;

    public class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(255);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(255);

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("Birth date is required");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required");

            When(x => x.NewImageFiles != null, () =>
            {
                RuleForEach(x => x.NewImageFiles)
                    .Must(file => file != null && file.Length > 0 && file.Length < 5242880)
                    .WithMessage("Each image file must be less than 5MB");
            });
        }
    }
}