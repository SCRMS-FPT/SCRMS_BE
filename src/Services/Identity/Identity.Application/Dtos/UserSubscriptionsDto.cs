using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Dtos
{
    public record UserSubscriptionsDto(
        Guid UserId,
        List<UserSubscriptionDto> Subscriptions
    );
    public record UserSubscriptionDto(
    Guid SubscriptionId,
    Guid PackageId,
    string PackageName,
    decimal PackagePrice,
    int PackageDurationDays,
    string AssociatedRole,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    DateTime CreatedAt
);
}