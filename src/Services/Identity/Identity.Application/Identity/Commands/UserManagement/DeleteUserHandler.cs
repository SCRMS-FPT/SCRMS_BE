using Identity.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Identity.Commands.UserManagement
{
    public sealed class DeleteUserHandler : ICommandHandler<DeleteUserCommand, Unit>
    {
        private readonly UserManager<User> _userManager;

        public DeleteUserHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Unit> Handle(
            DeleteUserCommand command,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(command.UserId.ToString());
            if (user == null || user.IsDeleted)
            {
                throw new DomainException("User not found");
            }

            user.IsDeleted = true;
            await _userManager.UpdateAsync(user);
            return Unit.Value;
        }
    }
}
