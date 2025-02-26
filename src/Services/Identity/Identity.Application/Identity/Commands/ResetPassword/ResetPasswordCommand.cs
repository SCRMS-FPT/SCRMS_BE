using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Identity.Commands.ResetPassword
{
    public record ResetPasswordCommand(
        string Email
  ) : ICommand<Unit>;

    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Old password is required");
        }
    }
}
