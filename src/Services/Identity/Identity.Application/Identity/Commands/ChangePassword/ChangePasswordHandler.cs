using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.Identity.Commands.ChangePassword
{
    public sealed class ChangePasswordHandler : ICommandHandler<ChangePasswordCommand, Unit>
    {
        private readonly IUserRepository _userRepository;

        public ChangePasswordHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(command.UserId);
            if (user == null)
                throw new DomainException("User not found");

            var result = await _userRepository.UpdatePasswordAsync(user, command.OldPassword, command.NewPassword);
            if (!result.Succeeded)
            {
                throw new DomainException($"Failed to change password: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return Unit.Value;
        }
    }
}