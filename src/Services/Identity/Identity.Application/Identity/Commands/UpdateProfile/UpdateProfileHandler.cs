using Identity.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Identity.Commands.UpdateProfile
{
    public sealed class UpdateProfileHandler : ICommandHandler<UpdateProfileCommand, UserDto>
    {
        private readonly UserManager<User> _userManager;

        public UpdateProfileHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UserDto> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(command.UserId.ToString());
            if (user == null)
                throw new DomainException("User not found");

            // Cập nhật thông tin người dùng
            user.FirstName = command.FirstName;
            user.LastName = command.LastName;
            user.PhoneNumber = command.Phone;
            user.BirthDate = command.BirthDate;
            // Chuyển đổi string sang enum (giả sử enum Gender đã được định nghĩa)
            user.Gender = Enum.Parse<Gender>(command.Gender, true);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new DomainException($"Failed to update profile: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return new UserDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.BirthDate,
                user.Gender.ToString(),
                user.MembershipStatus,
                user.CreatedAt
            );
        }
    }
}
