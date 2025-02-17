using Identity.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Identity.Commands.Register
{
    public sealed class RegisterUserHandler : ICommandHandler<RegisterUserCommand, RegisterUserResult>
    {
        private readonly UserManager<User> _userManager;

        public RegisterUserHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<RegisterUserResult> Handle(
            RegisterUserCommand command,
            CancellationToken cancellationToken)
        {
            var user = new User
            {
                FirstName = command.FirstName,
                LastName = command.LastName,
                Email = command.Email,
                UserName = command.Email,
                PhoneNumber = command.Phone,
                BirthDate = command.BirthDate,
                Gender = Enum.Parse<Gender>(command.Gender),
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, command.Password);

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
