using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.Application.ServicePackages.Commands.SubscribeToServicePackage;
using Identity.Application.Exceptions;
using Identity.Domain.Models;
using Identity.Test.Fakes;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Identity.Application.Data.Repositories;

namespace Identity.Test.Application.ServicePackages
{
    public class SubscribeToServicePackageHandlerTests
    {
        private DbContextOptions<IdentityDbContext> GetOptions(string dbName) => new DbContextOptionsBuilder<IdentityDbContext>().UseInMemoryDatabase(databaseName: dbName).Options;

        [Fact]
        public async Task Handle_ShouldSubscribeSuccessfullyAndAssignRole()
        {
            // Arrange
            var options = GetOptions("SubscribeTest");
            using var context = new IdentityDbContext(options);

            // Seed a service package
            var package = ServicePackage.Create("Test Package", "Description", 100, 30, "Premium");
            context.ServicePackages.Add(package);
            await context.SaveChangesAsync();

            // Setup UserRepository using Moq for user-related operations
            var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", UserName = "test@example.com" };
            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(x => x.GetUserByIdAsync(user.Id))
                .ReturnsAsync(user);
            userRepoMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new System.Collections.Generic.List<string> { "User" });
            userRepoMock.Setup(x => x.AddToRoleAsync(user, package.AssociatedRole))
                .ReturnsAsync(IdentityResult.Success);

            var handler = new SubscribeToServicePackageHandler(
                packageRepository: new FakeServicePackageRepository(context),
                subscriptionRepository: new FakeSubscriptionRepository(context),
                userRepository: userRepoMock.Object);

            var command = new SubscribeToServicePackageCommand(user.Id, package.Id);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.PackageId.Should().Be(package.Id);
            result.AssignedRole.Should().Be("Premium");

            var subscription = await context.Subscriptions.FirstOrDefaultAsync();
            subscription.Should().NotBeNull();
            subscription.Status.Should().Be("active");
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenPackageNotFound()
        {
            // Arrange
            var options = GetOptions("SubscribeTest_NotFound");
            using var context = new IdentityDbContext(options);

            var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", UserName = "test@example.com" };
            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(x => x.GetUserByIdAsync(user.Id))
                .ReturnsAsync(user);

            var handler = new SubscribeToServicePackageHandler(
                packageRepository: new FakeServicePackageRepository(context),
                subscriptionRepository: new FakeSubscriptionRepository(context),
                userRepository: userRepoMock.Object);

            var command = new SubscribeToServicePackageCommand(user.Id, Guid.NewGuid());

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>().WithMessage("Service package not found");
        }
    }
}