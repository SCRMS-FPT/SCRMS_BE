using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.ServicePackages.Commands.RenewSubscription
{
    public record RenewSubscriptionCommand(
        Guid SubscriptionId,
        Guid UserId,
        int AdditionalDurationDays
    ) : ICommand;
}