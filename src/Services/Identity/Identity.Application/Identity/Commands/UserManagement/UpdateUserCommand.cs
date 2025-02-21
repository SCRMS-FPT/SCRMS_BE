using FluentValidation;

namespace Identity.Application.Identity.Commands.UserManagement
{
    public record UpdateUserCommand(
        Guid UserId,
        string FirstName,
        string LastName,
        DateTime BirthDate,
        Gender Gender
    ) : ICommand<UserDto>;

    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(255);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(255);
            RuleFor(x => x.BirthDate).LessThan(DateTime.UtcNow.AddYears(-12));
        }
    }
}