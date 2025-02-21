using Mapster;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.Identity.Queries.UserManagement
{
    public class GetUserByIdQueryHandler(
        UserManager<User> userManager)
        : IQueryHandler<GetUserByIdQuery, UserDto?>
    {
        public async Task<UserDto?> Handle(
            GetUserByIdQuery request,
            CancellationToken cancellationToken)
        {
            var user = await userManager.Users
                .Where(u => u.Id == request.UserId && !u.IsDeleted)
                .ProjectToType<UserDto>()
                .FirstOrDefaultAsync(cancellationToken);

            return user;
        }
    }
}