using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.Identity.Commands.ResetPassword
{
    public sealed class ResetPasswordHandler : ICommandHandler<ResetPasswordCommand, Unit>
    {
        private readonly IUserRepository _userRepository;

        public ResetPasswordHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByEmailAsync(command.Email);
            if (user == null)
                throw new DomainException("User not found");

            return Unit.Value;
        }
    }
}