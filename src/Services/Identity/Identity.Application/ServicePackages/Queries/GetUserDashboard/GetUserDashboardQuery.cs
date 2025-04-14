namespace Identity.Application.ServicePackages.Queries.GetUserDashboard
{
    public record GetUserDashboardQuery(
        Guid UserId
    ) : IQuery<UserDashboardDto>;
}