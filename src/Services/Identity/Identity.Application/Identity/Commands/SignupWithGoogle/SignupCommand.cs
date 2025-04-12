using FluentValidation;

namespace Identity.Application.Identity.Commands.SignupWithGoogle
{
    public record SignupCommand(string Token,
        string Gender,
        string Phone,
        DateTime BirthDate) : ICommand<SignupResult>;

    public record SignupResult(Guid Id);

    public class SignupCommandValidator : AbstractValidator<SignupCommand>
    {
        public SignupCommandValidator()
        { RuleFor(x => x.Token).NotEmpty().WithMessage("Token is required."); }
    }
}
