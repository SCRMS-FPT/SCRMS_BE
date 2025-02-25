using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.ServicePackages.Commands.CancelSubscription;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Identity.Infrastructure.Data;

namespace Identity.Test.Application.ServicePackages
{
    public class CancelSubscriptionHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldCancelSubscriptionSuccessfully()
        { // Arrange: Use InMemoryDatabase cho IApplicationDbContext
            var options = new DbContextOptionsBuilder<IdentityDbContext>().UseInMemoryDatabase(databaseName: "CancelSubscriptionTest").Options;
            using var context = new IdentityDbContext(options);
            // Seed subscription
            var subscription = new ServicePackageSubscription
            {
                UserId = Guid.NewGuid(),
                PackageId = Guid.NewGuid(),
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Subscriptions.Add(subscription);
            await context.SaveChangesAsync();

            var handler = new CancelSubscriptionHandler(context);
            var command = new CancelSubscriptionCommand(subscription.Id, subscription.UserId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            var updated = await context.Subscriptions.FindAsync(subscription.Id);
            updated.Status.Should().Be("cancelled");
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenSubscriptionNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase(databaseName: "CancelSubscriptionTest_NotFound")
                .Options;

            using var context = new IdentityDbContext(options);
            var handler = new CancelSubscriptionHandler(context);
            var command = new CancelSubscriptionCommand(Guid.NewGuid(), Guid.NewGuid());

            // Act
            Func<Task> act = async () => { await handler.Handle(command, CancellationToken.None); };

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("Subscription not found or unauthorized");
        }
    }
}