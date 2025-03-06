using FluentValidation;

namespace Identity.Application.Identity.Commands.ResetPassword
{
    public record ResetPasswordCommand(
        string Email
  ) : ICommand<Unit>;

    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required");
        }
    }
}
