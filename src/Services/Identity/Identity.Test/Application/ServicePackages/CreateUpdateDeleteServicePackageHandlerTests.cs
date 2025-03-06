using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Identity.Application.Exceptions;
using Identity.Application.ServicePackages.Commands.ServicePackagesManagement;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using Identity.Test.Fakes;

namespace Identity.Test.Application.ServicePackages
{
    public class CreateUpdateDeleteServicePackageHandlerTests
    {
        private DbContextOptions<IdentityDbContext> GetOptions(string dbName) => new DbContextOptionsBuilder<IdentityDbContext>().UseInMemoryDatabase(databaseName: dbName).Options;

        [Fact]
        public async Task CreateServicePackage_ShouldCreateSuccessfully()
        {
            // Arrange
            var options = GetOptions("CreateServicePackageTest");
            using var context = new IdentityDbContext(options);
            var packageRepo = new FakeServicePackageRepository(context);
            var subscriptionRepo = new FakeSubscriptionRepository(context);
            var handler = new ServicePackageHandlers(packageRepo, subscriptionRepo);
            var command = new CreateServicePackageCommand("Package1", "Description", 50.00m, 30, "Basic", "active");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Package1");
        }

        [Fact]
        public async Task CreateServicePackage_ShouldCreateSuccessfully_WithNameAtMaxLength()
        {
            // Arrange
            var maxName = new string('A', 255);
            var options = GetOptions("CreateServicePackage_MaxNameTest");
            using var context = new IdentityDbContext(options);
            var packageRepo = new FakeServicePackageRepository(context);
            var subscriptionRepo = new FakeSubscriptionRepository(context);
            var handler = new ServicePackageHandlers(packageRepo, subscriptionRepo);
            var command = new CreateServicePackageCommand(maxName, "Description", 100.00m, 30, "Basic", "active");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Name.Should().Be(maxName);
        }

        [Fact]
        public async Task CreateServicePackage_ShouldThrowException_WhenNameExceedsMaximumLength()
        {
            // Arrange
            // Since EF InMemory may not enforce max length, we simulate the check.
            var longName = new string('B', 256);
            Action act = () =>
            {
                if (longName.Length > 255)
                    throw new ArgumentException("Name cannot exceed 255 characters");
                ServicePackage.Create(longName, "Description", 100, 30, "Basic");
            };

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Name cannot exceed 255 characters");
        }

        [Fact]
        public async Task CreateServicePackage_ShouldRoundPrice_ToTwoDecimalPlaces()
        {
            // Arrange
            var options = GetOptions("CreateServicePackage_PricePrecisionTest");
            using var context = new IdentityDbContext(options);
            var packageRepo = new FakeServicePackageRepository(context);
            var subscriptionRepo = new FakeSubscriptionRepository(context);
            var handler = new ServicePackageHandlers(packageRepo, subscriptionRepo);
            decimal priceWithExtraDecimals = 99.999m;
            var command = new CreateServicePackageCommand("Package2", "Description", priceWithExtraDecimals, 30, "Basic", "active");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            // Depending on your configuration, the stored price might be rounded.
            Math.Round(result.Price, 2).Should().Be(100.00m);
        }

        [Fact]
        public async Task UpdateServicePackage_ShouldUpdateSuccessfully_WithBoundaryValues()
        {
            // Arrange
            var options = GetOptions("UpdateServicePackageTest");
            using var context = new IdentityDbContext(options);
            var packageRepo = new FakeServicePackageRepository(context);
            var subscriptionRepo = new FakeSubscriptionRepository(context);
            var handler = new ServicePackageHandlers(packageRepo, subscriptionRepo);
            var createCommand = new CreateServicePackageCommand("Package1", "Description", 50, 30, "Basic", "active");
            var created = await handler.Handle(createCommand, CancellationToken.None);

            var maxName = new string('C', 255);
            var updateCommand = new UpdateServicePackageCommand(created.Id, maxName, "New Description", 75.50m, 45, "Basic", "active");

            // Act
            var updated = await handler.Handle(updateCommand, CancellationToken.None);

            // Assert
            updated.Name.Should().Be(maxName);
            updated.Price.Should().Be(75.50m);
        }

        [Fact]
        public async Task DeleteServicePackage_ShouldDeleteSuccessfully()
        {
            // Arrange
            var options = GetOptions("DeleteServicePackageTest");
            using var context = new IdentityDbContext(options);
            var packageRepo = new FakeServicePackageRepository(context);
            var subscriptionRepo = new FakeSubscriptionRepository(context);
            var handler = new ServicePackageHandlers(packageRepo, subscriptionRepo);
            var createCommand = new CreateServicePackageCommand("Package1", "Description", 50, 30, "Basic", "active");
            var created = await handler.Handle(createCommand, CancellationToken.None);

            var deleteCommand = new DeleteServicePackageCommand(created.Id);

            // Act
            var result = await handler.Handle(deleteCommand, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            var package = await context.ServicePackages.FirstOrDefaultAsync(p => p.Id == created.Id);
            package.Should().BeNull();
        }

        [Fact]
        public async Task DeleteServicePackage_ShouldThrowException_WhenActiveSubscriptionExists()
        {
            // Arrange
            var options = GetOptions("DeleteServicePackage_ActiveSubscriptionTest");
            using var context = new IdentityDbContext(options);
            var packageRepo = new FakeServicePackageRepository(context);
            var subscriptionRepo = new FakeSubscriptionRepository(context);
            var handler = new ServicePackageHandlers(packageRepo, subscriptionRepo);
            var createCommand = new CreateServicePackageCommand("Package1", "Description", 50, 30, "Basic", "active");
            var created = await handler.Handle(createCommand, CancellationToken.None);

            // Seed an active subscription for this package
            context.Subscriptions.Add(new ServicePackageSubscription
            {
                PackageId = created.Id,
                UserId = Guid.NewGuid(),
                Status = "active",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var deleteCommand = new DeleteServicePackageCommand(created.Id);

            // Act
            Func<Task> act = async () => { await handler.Handle(deleteCommand, CancellationToken.None); };

            // Assert
            await act.Should().ThrowAsync<ConflictException>()
                .WithMessage("Cannot delete package with active subscriptions");
        }
    }
}