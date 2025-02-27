using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Identity.Application.Exceptions;
using Identity.Application.ServicePackages.Commands.ServicePackagesManagement;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace Identity.Test.Application.ServicePackages
{
    public class CreateUpdateDeleteServicePackageHandlerTests
    {
        [Fact]
        public async Task CreateServicePackage_ShouldCreateSuccessfully()
        {
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase("CreateServicePackageTest")
                .Options;

            using var context = new IdentityDbContext(options);
            var handler = new ServicePackageHandlers(context);
            var command = new CreateServicePackageCommand("Package1", "Description", 50, 30, "Basic");

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Name.Should().Be("Package1");
        }

        [Fact]
        public async Task UpdateServicePackage_ShouldUpdateSuccessfully()
        {
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase("UpdateServicePackageTest")
                .Options;

            using var context = new IdentityDbContext(options);
            var createHandler = new ServicePackageHandlers(context);
            var createCommand = new CreateServicePackageCommand("Package1", "Description", 50, 30, "Basic");
            var created = await createHandler.Handle(createCommand, CancellationToken.None);

            var updateCommand = new UpdateServicePackageCommand(created.Id, "Package1 Updated", "New Description", 60, 40, "Basic", "active");
            var updated = await createHandler.Handle(updateCommand, CancellationToken.None);

            updated.Name.Should().Be("Package1 Updated");
            updated.Description.Should().Be("New Description");
            updated.Should().Be("active");
        }

        [Fact]
        public async Task DeleteServicePackage_ShouldDeleteSuccessfully()
        {
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase("DeleteServicePackageTest")
                .Options;

            using var context = new IdentityDbContext(options);
            var handler = new ServicePackageHandlers(context);
            var createCommand = new CreateServicePackageCommand("Package1", "Description", 50, 30, "Basic");
            var created = await handler.Handle(createCommand, CancellationToken.None);

            var deleteCommand = new DeleteServicePackageCommand(created.Id);
            var result = await handler.Handle(deleteCommand, CancellationToken.None);

            result.Should().Be(Unit.Value);
            var package = await context.ServicePackages.FirstOrDefaultAsync(p => p.Id == created.Id);
            package.Should().BeNull();
        }

        [Fact]
        public async Task DeleteServicePackage_ShouldThrowException_WhenActiveSubscriptionExists()
        {
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase("DeleteServicePackageTest_ActiveSubscription")
                .Options;

            using var context = new IdentityDbContext(options);
            var handler = new ServicePackageHandlers(context);
            var createCommand = new CreateServicePackageCommand("Package1", "Description", 50, 30, "Basic");
            var created = await handler.Handle(createCommand, CancellationToken.None);

            context.Subscriptions.Add(new ServicePackageSubscription
            {
                PackageId = created.Id,
                UserId = Guid.NewGuid(),
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var deleteCommand = new DeleteServicePackageCommand(created.Id);
            Func<Task> act = async () => { await handler.Handle(deleteCommand, CancellationToken.None); };

            await act.Should().ThrowAsync<ConflictException>()
                .WithMessage("Cannot delete package with active subscriptions");
        }
    }
}