using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Mapster;

namespace Identity.Application.Identity.Queries.UserManagement
{
    public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, IEnumerable<UserDto>>
    {
        private readonly IUserRepository _userRepository;

        public GetUsersQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserDto>> Handle(
            GetUsersQuery request,
            CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAllAsync();
            var activeUsers = users.Where(u => !u.IsDeleted).ToList();

            var userDtos = new List<UserDto>();
            foreach (var user in activeUsers)
            {
                var roles = await _userRepository.GetRolesAsync(user);
                var userDto = user.Adapt<UserDto>();
                userDto = userDto with { Roles = roles.ToList() };
                userDtos.Add(userDto);
            }

            return userDtos;
        }
    }
}