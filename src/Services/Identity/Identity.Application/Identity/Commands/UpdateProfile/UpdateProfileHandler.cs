using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.Identity.Commands.UpdateProfile
{
    public sealed class UpdateProfileHandler : ICommandHandler<UpdateProfileCommand, UserDto>
    {
        private readonly IUserRepository _userRepository;

        public UpdateProfileHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user == null)
                throw new DomainException("User not found");

            user.FirstName = command.FirstName;
            user.LastName = command.LastName;
            user.PhoneNumber = command.Phone;
            user.BirthDate = command.BirthDate;
            user.Gender = Enum.Parse<Gender>(command.Gender, true);
            user.SelfIntroduction = command.SelfIntroduction;

            var result = await _userRepository.UpdateAsync(user);
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
                user.SelfIntroduction,
                user.CreatedAt
            );
        }
    }
}