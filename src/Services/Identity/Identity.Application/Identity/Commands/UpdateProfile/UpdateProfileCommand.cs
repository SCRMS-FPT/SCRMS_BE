using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Identity.Commands.UpdateProfile
{
    public record UpdateProfileCommand(
        Guid UserId,
        string FirstName,
        string LastName,
        string Phone,
        DateTime BirthDate,
        string Gender
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
        }
    }
}
