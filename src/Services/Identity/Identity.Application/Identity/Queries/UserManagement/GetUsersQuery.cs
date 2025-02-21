namespace Identity.Application.Identity.Queries.UserManagement
{
    public record GetUsersQuery : IQuery<IEnumerable<UserDto>>;
}