using Carter;
using Chat.API.Features.GetChatMessages;
using Chat.API.Data.Models;
using Chat.API.Data.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Chat.API.Test.Endpoint
{
    public class GetChatMessagesEndpointTests
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public GetChatMessagesEndpointTests()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    var messageRepoMock = new Mock<IChatMessageRepository>();
                    messageRepoMock.Setup(r => r.GetChatMessageByChatSessionIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
                                   .ReturnsAsync(new List<ChatMessage>());
                    services.AddSingleton(messageRepoMock.Object);
                    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetChatMessagesHandler).Assembly));
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
        public async Task GetChatMessages_ReturnsOk_WhenValidRequest()
        {
            // Arrange
            var chatSessionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _client.DefaultRequestHeaders.Authorization = new("Test", "Bearer token");
            TestAuthHandler.FakeUser = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "Test"));

            // Act
            var response = await _client.GetAsync($"/api/chats/{chatSessionId}/messages?page=1&limit=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = JsonSerializer.Deserialize<List<ChatMessage>>(await response.Content.ReadAsStringAsync());
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetChatMessages_ReturnsUnauthorized_WhenNoToken()
        {
            // Arrange
            var chatSessionId = Guid.NewGuid();
            TestAuthHandler.FakeUser = null;
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync($"/api/chats/{chatSessionId}/messages?page=1&limit=10");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetChatMessages_ReturnsBadRequest_WhenPageIsNegative()
        {
            // Arrange
            var chatSessionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _client.DefaultRequestHeaders.Authorization = new("Test", "Bearer token");
            TestAuthHandler.FakeUser = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "Test"));

            // Act
            var response = await _client.GetAsync($"/api/chats/{chatSessionId}/messages?page=-1&limit=10");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}