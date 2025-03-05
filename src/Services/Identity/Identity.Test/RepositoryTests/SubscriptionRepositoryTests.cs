// File: SubscriptionRepositoryTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Identity.Domain.Models;
using Identity.Test.Fakes;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Identity.Test.RepositoryTests
{
    public class SubscriptionRepositoryTests
    {
        private DbContextOptions<IdentityDbContext> GetOptions(string dbName) =>
            new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

        [Fact]
        public async Task GetSubscriptionByIdAsync_ReturnsSubscription_WhenExists()
        {
            // Arrange
            var options = GetOptions("Subscription_GetByIdTest");
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

            var repo = new FakeSubscriptionRepository(context);

            // Act
            var result = await repo.GetSubscriptionByIdAsync(subscription.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(subscription.Id);
        }

        [Fact]
        public async Task GetSubscriptionByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var options = GetOptions("Subscription_GetById_NotFound");
            using var context = new IdentityDbContext(options);
            var repo = new FakeSubscriptionRepository(context);

            // Act
            var result = await repo.GetSubscriptionByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetSubscriptionByUserIdAsync_ReturnsSubscriptions_WhenExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var options = GetOptions("Subscription_GetByUserIdTest");
            using var context = new IdentityDbContext(options);
            var subscriptions = new List<ServicePackageSubscription>
            {
                new ServicePackageSubscription { UserId = userId, PackageId = Guid.NewGuid(), Status = "active", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ServicePackageSubscription { UserId = userId, PackageId = Guid.NewGuid(), Status = "active", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(60), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            context.Subscriptions.AddRange(subscriptions);
            await context.SaveChangesAsync();
            var repo = new FakeSubscriptionRepository(context);

            // Act
            var result = await repo.GetSubscriptionByUserIdAsync(userId);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetSubscriptionByUserIdAsync_ReturnsEmptyList_WhenNoneExist()
        {
            // Arrange
            var options = GetOptions("Subscription_GetByUserId_Empty");
            using var context = new IdentityDbContext(options);
            var repo = new FakeSubscriptionRepository(context);

            // Act
            var result = await repo.GetSubscriptionByUserIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task AddSubscriptionAsync_AddsSubscriptionSuccessfully()
        {
            // Arrange
            var options = GetOptions("Subscription_AddTest");
            using var context = new IdentityDbContext(options);
            var repo = new FakeSubscriptionRepository(context);
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

            // Act
            await repo.AddSubscriptionAsync(subscription);

            // Assert
            var result = await context.Subscriptions.FindAsync(subscription.Id);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateSubscriptionAsync_UpdatesSuccessfully()
        {
            // Arrange
            var options = GetOptions("Subscription_UpdateTest");
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
            var repo = new FakeSubscriptionRepository(context);

            // Act
            subscription.Status = "cancelled";
            await repo.UpdateSubscriptionAsync(subscription);

            // Assert
            var updated = await context.Subscriptions.FindAsync(subscription.Id);
            updated.Status.Should().Be("cancelled");
        }

        [Fact]
        public async Task DeleteSubscriptionAsync_DeletesSuccessfully()
        {
            // Arrange
            var options = GetOptions("Subscription_DeleteTest");
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
            var repo = new FakeSubscriptionRepository(context);

            // Act
            await repo.DeleteSubscriptionAsync(subscription);

            // Assert
            var result = await context.Subscriptions.FindAsync(subscription.Id);
            result.Should().BeNull();
        }

        [Fact]
        public async Task ExistsSubscriptionByPackageIdAsync_ReturnsTrue_WhenExists()
        {
            // Arrange
            var packageId = Guid.NewGuid();
            var options = GetOptions("Subscription_ExistsTest");
            using var context = new IdentityDbContext(options);
            context.Subscriptions.Add(new ServicePackageSubscription
            {
                UserId = Guid.NewGuid(),
                PackageId = packageId,
                Status = "active",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            var repo = new FakeSubscriptionRepository(context);

            // Act
            var exists = await repo.ExistsSubscriptionByPackageIdAsync(packageId);

            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsSubscriptionByPackageIdAsync_ReturnsFalse_WhenNotExists()
        {
            // Arrange
            var options = GetOptions("Subscription_NotExistsTest");
            using var context = new IdentityDbContext(options);
            var repo = new FakeSubscriptionRepository(context);

            // Act
            var exists = await repo.ExistsSubscriptionByPackageIdAsync(Guid.NewGuid());

            // Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsSubscriptionByPackageIdAsync_ReturnsFalse_ForEmptyGuid()
        {
            // Arrange
            var options = GetOptions("Subscription_EmptyGuidTest");
            using var context = new IdentityDbContext(options);
            var repo = new FakeSubscriptionRepository(context);

            // Act
            var exists = await repo.ExistsSubscriptionByPackageIdAsync(Guid.Empty);

            // Assert
            exists.Should().BeFalse();
        }
    }
}