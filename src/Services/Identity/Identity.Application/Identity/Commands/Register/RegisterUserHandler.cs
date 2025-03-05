using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.Identity.Commands.Register
{
    public sealed class RegisterUserHandler : ICommandHandler<RegisterUserCommand, RegisterUserResult>
    {
        private readonly IUserRepository _userRepository;

        public RegisterUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<RegisterUserResult> Handle(
            RegisterUserCommand command,
            CancellationToken cancellationToken)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = command.FirstName,
                LastName = command.LastName,
                Email = command.Email,
                UserName = command.Email,
                PhoneNumber = command.Phone,
                BirthDate = command.BirthDate,
                Gender = Enum.Parse<Gender>(command.Gender),
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userRepository.CreateUserAsync(user, command.Password);

            if (!result.Succeeded)
            {
                throw new DomainException(
                    $"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                );
            }

            return new RegisterUserResult(user.Id);
        }
    }
}