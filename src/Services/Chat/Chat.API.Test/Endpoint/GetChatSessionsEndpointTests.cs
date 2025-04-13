using Carter;
using Chat.API.Data.Repositories;
using Chat.API.Features.GetChatSessions;
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
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Chat.API.Test.Endpoint
{
    public class GetChatSessionsEndpointTests
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;
        private readonly Mock<IChatSessionRepository> _sessionRepoMock;

        public GetChatSessionsEndpointTests()
        {
            _sessionRepoMock = new Mock<IChatSessionRepository>();

            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    _sessionRepoMock.Setup(r => r.GetChatSessionByUserIdAsync(It.IsAny<Guid>()))
                                   .ReturnsAsync(new List<ChatSessionResponse>());
                    services.AddSingleton(_sessionRepoMock.Object);
                    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetChatSessionsHandler).Assembly));
                    services.AddRouting();
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                    services.AddAuthorization();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints =>
                    {
                        // Register only the specific endpoint we want to test
                        var chatSessionsEndpoint = new GetChatSessionsEndpoint();
                        chatSessionsEndpoint.AddRoutes(endpoints);
                    });
                });

            _server = new TestServer(builder);
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task GetChatSessions_ReturnsOk_WhenValidRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "Bearer token");
            TestAuthHandler.FakeUser = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "Test"));

            _sessionRepoMock.Setup(r => r.GetChatSessionByUserIdAsync(userId))
                .ReturnsAsync(new List<ChatSessionResponse>());

            // Act
            var response = await _client.GetAsync("/api/chats?page=1&limit=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = JsonSerializer.Deserialize<List<ChatSessionResponse>>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetChatSessions_ReturnsUnauthorized_WhenNoToken()
        {
            // Arrange
            TestAuthHandler.FakeUser = null;
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync("/api/chats?page=1&limit=10");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetChatSessions_ReturnsBadRequest_WhenLimitIsZero()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "Bearer token");
            TestAuthHandler.FakeUser = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "Test"));

            // Act
            var response = await _client.GetAsync("/api/chats?page=1&limit=0");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}