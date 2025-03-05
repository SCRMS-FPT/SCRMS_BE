using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.ServicePackages.Commands.SubscribeToServicePackage
{
    public class SubscribeToServicePackageHandler : ICommandHandler<SubscribeToServicePackageCommand, SubscribeToServicePackageResult>
    {
        private readonly IServicePackageRepository _packageRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IUserRepository _userRepository;

        public SubscribeToServicePackageHandler(
            IServicePackageRepository packageRepository,
            ISubscriptionRepository subscriptionRepository,
            IUserRepository userRepository)
        {
            _packageRepository = packageRepository;
            _subscriptionRepository = subscriptionRepository;
            _userRepository = userRepository;
        }

        public async Task<SubscribeToServicePackageResult> Handle(SubscribeToServicePackageCommand command, CancellationToken cancellationToken)
        {
            var package = await _packageRepository.GetByIdAsync(command.PackageId);
            if (package == null)
                throw new DomainException("Service package not found");

            if (package.Status != "active")
                throw new DomainException("Cannot subscribe to an inactive service package");

            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user == null)
                throw new DomainException("User not found");

            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(package.DurationDays);

            var subscription = new ServicePackageSubscription
            {
                UserId = command.UserId,
                PackageId = command.PackageId,
                StartDate = startDate,
                EndDate = endDate,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _subscriptionRepository.AddAsync(subscription);

            if (!(await _userRepository.GetRolesAsync(user)).Contains(package.AssociatedRole))
            {
                var result = await _userRepository.AddToRoleAsync(user, package.AssociatedRole);
                if (!result.Succeeded)
                    throw new DomainException($"Failed to assign role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return new SubscribeToServicePackageResult(
                subscription.Id,
                subscription.PackageId,
                subscription.StartDate,
                subscription.EndDate,
                subscription.Status,
                package.AssociatedRole
            );
        }
    }
}