using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Identity.Application.Identity.Commands.Register;
using Identity.Domain.Exceptions;
using Mapster;

namespace Identity.Application.Identity.Commands.UserManagement
{
    public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UserDto>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> Handle(
            UpdateUserCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null || user.IsDeleted)
            {
                throw new UserNotFoundException(request.UserId);
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.BirthDate = request.BirthDate;
            user.Gender = Enum.Parse<Gender>(request.Gender);
            user.SelfIntroduction = request.SelfIntroduction;
            user.PhoneNumber = request.Phone;

            await _userRepository.UpdateUserAsync(user);
            var roles = await _userRepository.GetRolesAsync(user);

            var userDto = new UserDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.BirthDate,
                user.Gender.ToString(),
                user.SelfIntroduction,
                user.CreatedAt,
                roles.ToList(),
                user.GetImageUrlsList()
            );
            return userDto;
        }
    }
}