using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.ServicePackages.Commands.CancelSubscription
{
    public record CancelSubscriptionCommand(
        Guid SubscriptionId,
        Guid UserId
    ) : ICommand;
}