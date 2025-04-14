using Identity.Application.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Application.ServicePackages.Queries.GetUserDashboard
{
    public class GetUserDashboardQueryHandler : IQueryHandler<GetUserDashboardQuery, UserDashboardDto>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IServicePackageRepository _packageRepository;
        private readonly UserManager<User> _userManager;

        public GetUserDashboardQueryHandler(
            ISubscriptionRepository subscriptionRepository,
            IServicePackageRepository packageRepository,
            UserManager<User> userManager)
        {
            _subscriptionRepository = subscriptionRepository;
            _packageRepository = packageRepository;
            _userManager = userManager;
        }

        public async Task<UserDashboardDto> Handle(GetUserDashboardQuery query, CancellationToken cancellationToken)
        {
            // Get user roles
            var user = await _userManager.FindByIdAsync(query.UserId.ToString());
            if (user == null)
            {
                throw new Exception($"User {query.UserId} not found");
            }
            var roles = (await _userManager.GetRolesAsync(user)).ToList();

            // Get user subscriptions
            var subscriptions = await _subscriptionRepository.GetSubscriptionByUserIdAsync(query.UserId);
            var now = DateTime.UtcNow;

            // Map subscriptions to DTOs with additional information
            var subscriptionDtos = new List<UserSubscriptionInfoDto>();

            foreach (var subscription in subscriptions)
            {
                // Get the package details for each subscription
                var package = await _packageRepository.GetServicePackageByIdAsync(subscription.PackageId);
                if (package == null) continue;

                var daysRemaining = (subscription.EndDate - now).Days;
                var isExpired = now > subscription.EndDate || subscription.Status == "expired";

                subscriptionDtos.Add(new UserSubscriptionInfoDto(
                    subscription.Id,
                    subscription.PackageId,
                    package.Name,
                    package.Description,
                    package.Price,
                    package.DurationDays,
                    package.AssociatedRole,
                    subscription.StartDate,
                    subscription.EndDate,
                    subscription.Status,
                    daysRemaining > 0 ? daysRemaining : 0,
                    isExpired,
                    subscription.CreatedAt
                ));
            }

            return new UserDashboardDto(
                query.UserId,
                roles,
                subscriptionDtos
            );
        }
    }
}