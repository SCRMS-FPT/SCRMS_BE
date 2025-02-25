using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.ServicePackages.Queries.GetUserSubscriptions
{
    public record GetUserSubscriptionsQuery(Guid UserId) : IQuery<UserSubscriptionsDto>;
}