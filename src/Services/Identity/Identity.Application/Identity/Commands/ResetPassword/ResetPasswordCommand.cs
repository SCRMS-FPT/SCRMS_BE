using FluentValidation;

namespace Identity.Application.Identity.Commands.ResetPassword
{
    public record RequestPasswordResetCommand(
        string Email
    ) : ICommand<Unit>;

    public class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
    {
        public RequestPasswordResetCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required");
        }
    }

    public record ConfirmPasswordResetCommand(
        string Token, string NewPassword) : ICommand<Unit>;

    public class ConfirmPasswordResetCommandValidator : AbstractValidator<ConfirmPasswordResetCommand>
    {
        public ConfirmPasswordResetCommandValidator()
        {
            RuleFor(x => x.Token).NotEmpty().WithMessage("Token is required");
            RuleFor(x => x.NewPassword).NotEmpty().WithMessage("New password is required");
        }
    }
}
