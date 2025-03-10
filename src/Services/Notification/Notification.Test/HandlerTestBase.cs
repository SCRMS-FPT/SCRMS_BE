using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Notification.API.Data;
using Notification.API.Hubs;
using Notification.API.Services;

namespace Notification.Test
{
    public abstract class HandlerTestBase : IDisposable
    {
        protected readonly NotificationDbContext Context;
        protected readonly Mock<IMediator> MediatorMock;
        protected readonly Mock<IHubContext<NotifyHub>> HubContextMock;
        protected readonly Mock<IEmailService> EmailServiceMock;
        protected readonly Mock<ISender> SenderMock;

        protected HandlerTestBase()
        {
            var options = new DbContextOptionsBuilder<NotificationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            Context = new NotificationDbContext(options);
            MediatorMock = new Mock<IMediator>();
            HubContextMock = new Mock<IHubContext<NotifyHub>>();
            EmailServiceMock = new Mock<IEmailService>();
            SenderMock = new Mock<ISender>();

            // Configure HubContextMock for SignalR operations
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();

            // When Clients is accessed, return the IHubClients mock
            HubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

            // When User(string) is called on IHubClients, return the IClientProxy mock
            clientsMock.Setup(c => c.User(It.IsAny<string>())).Returns(clientProxyMock.Object);

            // Configure SendCoreAsync (the underlying method) instead of SendAsync
            clientProxyMock.Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }
    }
}
