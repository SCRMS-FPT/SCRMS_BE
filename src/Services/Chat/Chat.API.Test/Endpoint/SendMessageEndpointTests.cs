using Carter;
using Chat.API.Features.SendMessage;
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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Chat.API.Test.Endpoint
{
    public class SendMessageEndpointTests
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public SendMessageEndpointTests()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    var messageRepoMock = new Mock<IChatMessageRepository>();
                    messageRepoMock.Setup(r => r.AddChatMessageAsync(It.IsAny<ChatMessage>()))
                        .Returns(Task.CompletedTask);
                    services.AddSingleton(messageRepoMock.Object);
                    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SendMessageHandler).Assembly));
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
        public async Task SendMessage_ReturnsCreated_WhenValidRequest()
        {
            // Arrange
            var chatSessionId = Guid.NewGuid();
            var request = new SendMessageRequest("Hello");
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new("Test", "Bearer token");
            TestAuthHandler.FakeUser = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }, "Test"));

            // Act
            var response = await _client.PostAsync($"/api/chats/{chatSessionId}/messages", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = JsonSerializer.Deserialize<ChatMessage>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.Equal("Hello", result.MessageText);
        }

        [Fact]
        public async Task SendMessage_ReturnsUnauthorized_WhenNoToken()
        {
            // Arrange
            var chatSessionId = Guid.NewGuid();
            var request = new SendMessageRequest("Hello");
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            TestAuthHandler.FakeUser = null;
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.PostAsync($"/api/chats/{chatSessionId}/messages", content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task SendMessage_ReturnsBadRequest_WhenMessageTextIsEmpty()
        {
            // Arrange
            var chatSessionId = Guid.NewGuid();
            var request = new SendMessageRequest("");
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new("Test", "Bearer token");
            TestAuthHandler.FakeUser = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }, "Test"));

            // Act
            var response = await _client.PostAsync($"/api/chats/{chatSessionId}/messages", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}