using System;
using System.Collections.Generic;

namespace Identity.Application.ServicePackages.Queries.GetUserDashboard
{
    public record UserDashboardDto(
        Guid UserId,
        List<string> Roles,
        List<UserSubscriptionInfoDto> Subscriptions
    );

    public record UserSubscriptionInfoDto(
        Guid Id,
        Guid PackageId,
        string PackageName,
        string Description,
        decimal Price,
        int DurationDays,
        string AssociatedRole,
        DateTime StartDate,
        DateTime EndDate,
        string Status,
        int DaysRemaining,
        bool IsExpired,
        DateTime CreatedAt
    );
}