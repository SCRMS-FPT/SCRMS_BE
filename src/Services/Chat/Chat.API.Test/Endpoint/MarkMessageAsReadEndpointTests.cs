using Carter;
using Chat.API.Features.MarkMessageAsRead;
using Chat.API.Data.Models;
using Chat.API.Data.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Chat.API.Test.Endpoint
{
    public class MarkMessageAsReadEndpointTests
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public MarkMessageAsReadEndpointTests()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    var messageRepoMock = new Mock<IChatMessageRepository>();
                    messageRepoMock.Setup(r => r.GetChatMessageByIdAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(new ChatMessage());
                    messageRepoMock.Setup(r => r.UpdateChatMessageAsync(It.IsAny<ChatMessage>()))
                        .Returns(Task.CompletedTask);
                    services.AddSingleton(messageRepoMock.Object);
                    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MarkMessageAsReadHandler).Assembly));
                    services.AddCarter();
                    services.AddRouting();
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                    services.AddAuthorization();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapCarter();
                    });
                });

            _server = new TestServer(builder);
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task MarkMessageAsRead_ReturnsOk_WhenValidRequest()
        {
            // Arrange
            var chatSessionId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            _client.DefaultRequestHeaders.Authorization = new("Test", "Bearer token");
            TestAuthHandler.FakeUser = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }, "Test"));

            // Act
            var response = await _client.PostAsync($"/api/chats/{chatSessionId}/messages/{messageId}/read", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task MarkMessageAsRead_ReturnsUnauthorized_WhenNoToken()
        {
            // Arrange
            var chatSessionId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            TestAuthHandler.FakeUser = null;
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.PostAsync($"/api/chats/{chatSessionId}/messages/{messageId}/read", null);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task MarkMessageAsRead_ReturnsBadRequest_WhenMessageIdIsEmpty()
        {
            // Arrange
            var chatSessionId = Guid.NewGuid();
            _client.DefaultRequestHeaders.Authorization = new("Test", "Bearer token");
            TestAuthHandler.FakeUser = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }, "Test"));

            // Act
            var response = await _client.PostAsync($"/api/chats/{chatSessionId}/messages/{Guid.Empty}/read", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}