using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.ServicePackages.Commands.RenewSubscription
{
    public class RenewSubscriptionHandler : ICommandHandler<RenewSubscriptionCommand, Unit>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;

        public RenewSubscriptionHandler(ISubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<Unit> Handle(RenewSubscriptionCommand command, CancellationToken cancellationToken)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(command.SubscriptionId);
            if (subscription == null || subscription.UserId != command.UserId)
                throw new DomainException("Subscription not found or unauthorized");

            subscription.EndDate = subscription.EndDate.AddDays(command.AdditionalDurationDays);
            subscription.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepository.UpdateAsync(subscription);

            return Unit.Value;
        }
    }
}