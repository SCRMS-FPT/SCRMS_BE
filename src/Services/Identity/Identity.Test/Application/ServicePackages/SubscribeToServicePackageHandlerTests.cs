using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.ServicePackages.Commands.SubscribeToServicePackage;
using Microsoft.EntityFrameworkCore;

namespace Identity.Test.Application.ServicePackages
{
    public class SubscribeToServicePackageHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldSubscribeSuccessfullyAndAssignRole()
        {
            var options = new DbContextOptionsBuilder<IdentityDbContext>().UseInMemoryDatabase("SubscribeTest").Options; using var context = new IdentityDbContext(options);

            // Seed một service package
            var package = ServicePackage.Create("Test Package", "Description", 100, 30, "Premium");
            context.ServicePackages.Add(package);
            await context.SaveChangesAsync();

            // Setup UserManager
            var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", UserName = "test@example.com" };
            var userManagerMock = new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.IsInRoleAsync(user, "Premium"))
                .ReturnsAsync(false);
            userManagerMock.Setup(x => x.AddToRoleAsync(user, "Premium"))
                .ReturnsAsync(IdentityResult.Success);

            var handler = new SubscribeToServicePackageHandler(context, userManagerMock.Object);
            var command = new SubscribeToServicePackageCommand(user.Id, package.Id);

            var result = await handler.Handle(command, CancellationToken.None);

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
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase("SubscribeTest_NotFound")
                .Options;
            using var context = new IdentityDbContext(options);

            var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", UserName = "test@example.com" };
            var userManagerMock = new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);

            var handler = new SubscribeToServicePackageHandler(context, userManagerMock.Object);
            var command = new SubscribeToServicePackageCommand(user.Id, Guid.NewGuid());

            Func<Task> act = async () => { await handler.Handle(command, CancellationToken.None); };

            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("Service package not found");
        }
    }
}