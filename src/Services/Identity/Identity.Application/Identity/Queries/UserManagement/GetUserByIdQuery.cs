namespace Identity.Application.Identity.Queries.UserManagement
{
    public record GetUserByIdQuery(Guid UserId) : IQuery<UserDto>;
    public record GetUserProfileByIdQuery(Guid UserId) : IQuery<UserProfileDto>;
}