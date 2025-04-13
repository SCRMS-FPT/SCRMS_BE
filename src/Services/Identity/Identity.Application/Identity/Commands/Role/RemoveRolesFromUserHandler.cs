using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.Identity.Commands.Role
{
    public sealed class RemoveRolesFromUserHandler : ICommandHandler<RemoveRolesFromUserCommand, Unit>
    {
        private readonly IUserRepository _userRepository;

        public RemoveRolesFromUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(
            RemoveRolesFromUserCommand command,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(command.UserId);
            if (user == null)
                throw new DomainException("User not found");

            await _userRepository.RemoveFromRolesAsync(user, command.Roles);

            return Unit.Value;
        }
    }
}