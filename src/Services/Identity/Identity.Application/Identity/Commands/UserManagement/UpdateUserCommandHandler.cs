using Mapster;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.Identity.Commands.UserManagement
{
    public class UpdateUserCommandHandler(
        UserManager<User> userManager)
        : ICommandHandler<UpdateUserCommand, UserDto>
    {
        public async Task<UserDto> Handle(
            UpdateUserCommand request,
            CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null || user.IsDeleted)
            {
                throw new UserNotFoundException(request.UserId);
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.BirthDate = request.BirthDate;
            user.Gender = request.Gender;

            await userManager.UpdateAsync(user);

            return user.Adapt<UserDto>();
        }
    }
}