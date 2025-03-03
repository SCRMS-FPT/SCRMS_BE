using Mapster;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.Identity.Queries.UserManagement
{
    public class GetUsersQueryHandler(
        UserManager<User> userManager)
        : IQueryHandler<GetUsersQuery, IEnumerable<UserDto>>
    {
        public async Task<IEnumerable<UserDto>> Handle(
            GetUsersQuery request,
            CancellationToken cancellationToken)
        {
            var users = await userManager.Users
                .Where(u => !u.IsDeleted)
                .ToListAsync(cancellationToken);

            // Tạo danh sách UserDto với thông tin roles
            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                // Lấy danh sách roles của user
                var roles = await userManager.GetRolesAsync(user);

                // Ánh xạ từ User sang UserDto và thêm roles
                var userDto = user.Adapt<UserDto>();
                userDto = userDto with { Roles = roles.ToList() }; // Gán danh sách roles

                userDtos.Add(userDto);
            }

            return userDtos;
        }
    }
}