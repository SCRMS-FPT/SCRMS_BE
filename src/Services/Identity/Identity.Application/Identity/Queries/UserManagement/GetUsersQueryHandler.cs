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
                .ProjectToType<UserDto>()
                .ToListAsync(cancellationToken);

            return users;
        }
    }
}