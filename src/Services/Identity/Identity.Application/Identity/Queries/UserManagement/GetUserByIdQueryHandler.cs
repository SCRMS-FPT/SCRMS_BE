using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Mapster;

namespace Identity.Application.Identity.Queries.UserManagement
{
    public class GetUserByIdQueryHandler :
        IQueryHandler<GetUserByIdQuery, UserDto?>,
        IQueryHandler<GetUserProfileByIdQuery, UserProfileDto?>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByIdQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // Handler for GetUserByIdQuery, returning UserDto
        public async Task<UserDto?> Handle(
            GetUserByIdQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            // Adapt to UserDto (full user details, including Roles and CreatedAt)
            return user.Adapt<UserDto>();
        }

        // Handler for GetUserProfileByIdQuery, returning UserProfileDto (without Roles and CreatedAt)
        public async Task<UserProfileDto?> Handle(
            GetUserProfileByIdQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            // Adapt to UserProfileDto (only the required fields, excluding Roles and CreatedAt)
            return user.Adapt<UserProfileDto>();
        }
    }
}