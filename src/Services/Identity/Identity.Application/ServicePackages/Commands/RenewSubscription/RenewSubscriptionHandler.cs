using System;
using System.Threading;
using System.Threading.Tasks;
using Identity.Domain.Exceptions;
using MediatR;

namespace Identity.Application.ServicePackages.Commands.RenewSubscription
{
    public class RenewSubscriptionHandler : ICommandHandler<RenewSubscriptionCommand, Unit>
    {
        private readonly IApplicationDbContext _dbContext;

        public RenewSubscriptionHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Unit> Handle(RenewSubscriptionCommand command, CancellationToken cancellationToken)
        {
            var subscription = await _dbContext.Subscriptions.FindAsync(command.SubscriptionId);
            if (subscription == null || subscription.UserId != command.UserId)
                throw new DomainException("Subscription not found or unauthorized");

            subscription.EndDate = subscription.EndDate.AddDays(command.AdditionalDurationDays);
            subscription.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Return Unit.Value to indicate successful completion with no result
            return Unit.Value;
        }
    }
}