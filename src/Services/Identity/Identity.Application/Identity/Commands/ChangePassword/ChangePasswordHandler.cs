using Identity.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.Identity.Commands.ChangePassword
{
    public sealed class ChangePasswordHandler : ICommandHandler<ChangePasswordCommand, Unit>
    {
        private readonly UserManager<User> _userManager;

        public ChangePasswordHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Unit> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(command.UserId.ToString());
            if (user == null)
                throw new DomainException("User not found");

            var result = await _userManager.ChangePasswordAsync(user, command.OldPassword, command.NewPassword);
            if (!result.Succeeded)
            {
                throw new DomainException($"Failed to change password: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return Unit.Value;
        }
    }
}