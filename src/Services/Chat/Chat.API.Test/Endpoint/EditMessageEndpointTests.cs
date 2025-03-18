using Carter;
using Chat.API.Features.EditMessage;
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
    public class EditMessageEndpointTests
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public EditMessageEndpointTests()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    var messageRepoMock = new Mock<IChatMessageRepository>();
                    messageRepoMock.Setup(r => r.GetChatMessageByIdAndSessionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                        .ReturnsAsync(new ChatMessage());
                    messageRepoMock.Setup(r => r.UpdateChatMessageAsync(It.IsAny<ChatMessage>()))
                        .Returns(Task.CompletedTask);
                    services.AddSingleton(messageRepoMock.Object);
                    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EditMessageHandler).Assembly));
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
        public async Task EditMessage_ReturnsOk_WhenValidRequest()
        {
            // Arrange
            var chatSessionId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            var request = new EditMessageRequest("Updated Text");
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new("Test", "Bearer token");

            // Sử dụng FakeUser tĩnh
            TestAuthHandler.FakeUser = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }, "Test"));

            // Act
            var response = await _client.PutAsync($"/api/chats/{chatSessionId}/messages/{messageId}", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task EditMessage_ReturnsUnauthorized_WhenNoToken()
        {
            // Arrange
            var chatSessionId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            var request = new EditMessageRequest("Updated Text");
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Đảm bảo không có token và FakeUser không được thiết lập
            TestAuthHandler.FakeUser = null;
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.PutAsync($"/api/chats/{chatSessionId}/messages/{messageId}", content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task EditMessage_ReturnsBadRequest_WhenMessageTextIsEmpty()
        {
            // Arrange
            var chatSessionId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            var request = new EditMessageRequest("");
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new("Test", "Bearer token");
            TestAuthHandler.FakeUser = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }, "Test"));

            // Act
            var response = await _client.PutAsync($"/api/chats/{chatSessionId}/messages/{messageId}", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}