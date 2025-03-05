using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Mapster;

namespace Identity.Application.ServicePackages.Queries.GetUserSubscriptions
{
    public class GetUserSubscriptionsHandler : IQueryHandler<GetUserSubscriptionsQuery, UserSubscriptionsDto>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;

        public GetUserSubscriptionsHandler(ISubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<UserSubscriptionsDto> Handle(GetUserSubscriptionsQuery query, CancellationToken cancellationToken)
        {
            var subscriptions = await _subscriptionRepository.GetByUserIdAsync(query.UserId);
            var subscriptionDtos = subscriptions.Select(s => new UserSubscriptionDto(
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
            )).ToList();

            return new UserSubscriptionsDto(query.UserId, subscriptionDtos);
        }
    }
}