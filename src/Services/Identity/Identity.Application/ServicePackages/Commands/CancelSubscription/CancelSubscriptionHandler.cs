using Identity.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Identity.Application.ServicePackages.Commands.CancelSubscription
{
    public class CancelSubscriptionHandler : ICommandHandler<CancelSubscriptionCommand, Unit>
    {
        private readonly IApplicationDbContext _dbContext;

        public CancelSubscriptionHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Unit> Handle(CancelSubscriptionCommand command, CancellationToken cancellationToken)
        {
            var subscription = await _dbContext.Subscriptions.FindAsync(command.SubscriptionId);
            if (subscription == null || subscription.UserId != command.UserId)
                throw new DomainException("Subscription not found or unauthorized");

            subscription.Status = "cancelled";
            subscription.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Returning Unit.Value to indicate that the operation completed successfully with no result.
            return Unit.Value;
        }
    }
}