using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.ServicePackages.Commands.SubscribeToServicePackage
{
    public record SubscribeToServicePackageCommand(
    Guid UserId,
    Guid PackageId
) : ICommand<SubscribeToServicePackageResult>;

    public record SubscribeToServicePackageResult(
        Guid SubscriptionId,
        Guid PackageId,
        DateTime StartDate,
        DateTime EndDate,
        string Status,
        string AssignedRole
    );
}