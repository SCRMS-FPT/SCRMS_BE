using FluentValidation;

namespace Identity.Application.Identity.Commands.Login
{
    public record LoginUserCommand(
        string Email,
        string Password
    ) : ICommand<LoginUserResult>;

    public record LoginUserResult(
        string Token,
        Guid UserId,
        string Email
    );

    public class LoginUserValidator : AbstractValidator<LoginUserCommand>
    {
        public LoginUserValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
}