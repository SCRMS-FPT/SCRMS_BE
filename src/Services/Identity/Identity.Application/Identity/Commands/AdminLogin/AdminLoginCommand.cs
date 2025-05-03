using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Identity.Commands.AdminLogin
{
    public record AdminLoginCommand(
        string Email,
        string Password
    ) : ICommand<AdminLoginResult>;

    public record AdminLoginResult(
        string Token,
        Guid UserId,
        UserDto User
    );

    public class LoginUserValidator : AbstractValidator<AdminLoginCommand>
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
