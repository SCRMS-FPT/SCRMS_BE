using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.Identity.Commands.UserManagement
{
    public sealed class DeleteUserHandler : ICommandHandler<DeleteUserCommand, Unit>
    {
        private readonly IUserRepository _userRepository;

        public DeleteUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(
            DeleteUserCommand command,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(command.UserId);
            if (user == null || user.IsDeleted)
            {
                throw new DomainException("User not found");
            }

            user.IsDeleted = true;
            await _userRepository.UpdateUserAsync(user);
            return Unit.Value;
        }
    }
}