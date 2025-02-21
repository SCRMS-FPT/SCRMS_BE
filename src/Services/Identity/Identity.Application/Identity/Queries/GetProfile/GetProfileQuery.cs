namespace Identity.Application.Identity.Queries.GetProfile
{
    public record GetProfileQuery(Guid UserId) : IQuery<UserDto>;
}