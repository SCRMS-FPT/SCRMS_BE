using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.Application.ServicePackages.Commands.RenewSubscription;
using Identity.Application.Exceptions;
using Identity.Domain.Models;
using Identity.Test.Fakes;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Identity.Test.Application.ServicePackages
{
    public class RenewSubscriptionHandlerTests
    {
        private DbContextOptions<IdentityDbContext> GetOptions(string dbName) => new DbContextOptionsBuilder<IdentityDbContext>().UseInMemoryDatabase(databaseName: dbName).Options;

        [Fact]
        public async Task Handle_ShouldRenewSubscriptionSuccessfully()
        {
            // Arrange
            var options = GetOptions("RenewSubscriptionTest");
            using var context = new IdentityDbContext(options);
            var subscription = new ServicePackageSubscription
            {
                UserId = Guid.NewGuid(),
                PackageId = Guid.NewGuid(),
                Status = "active",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Subscriptions.Add(subscription);
            await context.SaveChangesAsync();
            var originalEndDate = subscription.EndDate;

            var handler = new RenewSubscriptionHandler(new FakeSubscriptionRepository(context));
            var command = new RenewSubscriptionCommand(subscription.Id, subscription.UserId, 15);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            var updated = await context.Subscriptions.FindAsync(subscription.Id);
            updated.EndDate.Should().Be(originalEndDate.AddDays(15));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenSubscriptionNotFound()
        {
            // Arrange
            var options = GetOptions("RenewSubscriptionTest_NotFound");
            using var context = new IdentityDbContext(options);
            var handler = new RenewSubscriptionHandler(new FakeSubscriptionRepository(context));
            var command = new RenewSubscriptionCommand(Guid.NewGuid(), Guid.NewGuid(), 15);

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>().WithMessage("Subscription not found or unauthorized");
        }
    }
}