using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.Identity.Commands.Role
{
    public sealed class AssignRolesToUserHandler : ICommandHandler<AssignRolesToUserCommand, Unit>
    {
        private readonly IUserRepository _userRepository;

        public AssignRolesToUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(
            AssignRolesToUserCommand command,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user == null)
                throw new DomainException("User not found");

            var currentRoles = await _userRepository.GetRolesAsync(user);
            await _userRepository.RemoveFromRolesAsync(user, currentRoles);

            await _userRepository.AddToRolesAsync(user, command.Roles);

            return Unit.Value;
        }
    }
}