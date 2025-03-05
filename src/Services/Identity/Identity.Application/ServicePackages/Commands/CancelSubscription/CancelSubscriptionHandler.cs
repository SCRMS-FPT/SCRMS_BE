using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.ServicePackages.Commands.CancelSubscription
{
    public class CancelSubscriptionHandler : ICommandHandler<CancelSubscriptionCommand, Unit>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;

        public CancelSubscriptionHandler(ISubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<Unit> Handle(CancelSubscriptionCommand command, CancellationToken cancellationToken)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(command.SubscriptionId);
            if (subscription == null || subscription.UserId != command.UserId)
                throw new DomainException("Subscription not found or unauthorized");

            subscription.Status = "cancelled";
            subscription.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepository.UpdateAsync(subscription);

            return Unit.Value;
        }
    }
}