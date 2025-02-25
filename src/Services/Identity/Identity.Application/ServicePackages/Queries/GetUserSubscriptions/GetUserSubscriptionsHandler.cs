using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.ServicePackages.Queries.GetUserSubscriptions
{
    public class GetUserSubscriptionsHandler : IQueryHandler<GetUserSubscriptionsQuery, UserSubscriptionsDto>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetUserSubscriptionsHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserSubscriptionsDto> Handle(GetUserSubscriptionsQuery query, CancellationToken cancellationToken)
        {
            var subscriptions = await _dbContext.Subscriptions
                .Where(s => s.UserId == query.UserId)
                .Include(s => s.Package)
                .Select(s => new UserSubscriptionDto(
                    s.Id,
                    s.PackageId,
                    s.Package.Name,
                    s.Package.Price,
                    s.Package.DurationDays,
                    s.Package.AssociatedRole,
                    s.StartDate,
                    s.EndDate,
                    s.Status,
                    s.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return new UserSubscriptionsDto(query.UserId, subscriptions);
        }
    }
}