using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.ServicePackages.Commands.RenewSubscription;
using Microsoft.EntityFrameworkCore;

namespace Identity.Test.Application.ServicePackages
{
    public class RenewSubscriptionHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldRenewSubscriptionSuccessfully()
        {
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase("RenewSubscriptionTest")
                .Options;
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

            // Lưu lại giá trị ban đầu của EndDate
            var originalEndDate = subscription.EndDate;

            var handler = new RenewSubscriptionHandler(context);
            var command = new RenewSubscriptionCommand(subscription.Id, subscription.UserId, 15);

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().Be(Unit.Value);
            var updated = await context.Subscriptions.FindAsync(subscription.Id);
            updated.EndDate.Should().Be(originalEndDate.AddDays(15));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenSubscriptionNotFound()
        {
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase("RenewSubscriptionTest_NotFound")
                .Options;

            using var context = new IdentityDbContext(options);
            var handler = new RenewSubscriptionHandler(context);
            var command = new RenewSubscriptionCommand(Guid.NewGuid(), Guid.NewGuid(), 15);

            Func<Task> act = async () => { await handler.Handle(command, CancellationToken.None); };

            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("Subscription not found or unauthorized");
        }
    }
}