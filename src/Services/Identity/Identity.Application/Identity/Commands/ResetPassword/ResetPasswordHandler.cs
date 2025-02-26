using Identity.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.Identity.Commands.ResetPassword
{
    public sealed class ResetPasswordHandler : ICommandHandler<ResetPasswordCommand, Unit>
    {
        private readonly UserManager<User> _userManager;

        public ResetPasswordHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Unit> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(command.Email);
            if (user == null)
                throw new DomainException("User not found");

            // TODO: Send email but I don't find anything so this will be replace soon

            //var result = await _userManager.ChangePasswordAsync(user, command.OldPassword, command.NewPassword);
            //if (!result.Succeeded)
            //{
            //    throw new DomainException($"Failed to change password: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            //}

            return Unit.Value;
        }
    }
}
