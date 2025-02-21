using Identity.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.Identity.Commands.Role
{
    public sealed class AssignRolesToUserHandler : ICommandHandler<AssignRolesToUserCommand, Unit>
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public AssignRolesToUserHandler(
            UserManager<User> userManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<Unit> Handle(
            AssignRolesToUserCommand command,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(command.UserId.ToString());
            if (user == null) throw new DomainException("User not found");

            foreach (var role in command.Roles)
                if (!await _roleManager.RoleExistsAsync(role))
                    throw new DomainException($"Role '{role}' does not exist");

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRolesAsync(user, command.Roles);

            return Unit.Value;
        }
    }
}