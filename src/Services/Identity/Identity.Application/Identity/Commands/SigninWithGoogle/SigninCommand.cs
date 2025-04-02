using FluentValidation;

namespace Identity.Application.Identity.Commands.SigninWithGoogle
{
    public record SigninCommand(string Token) : ICommand<SigninResult>;
    public record SigninResult(string Token,
        Guid UserId,
        UserDto User);
    public class SigninCommandValidator : AbstractValidator<SigninCommand>
    {
        public SigninCommandValidator()
        {
            RuleFor(x => x.Token).NotEmpty().WithMessage("Token is required.");
        }
    }
}
