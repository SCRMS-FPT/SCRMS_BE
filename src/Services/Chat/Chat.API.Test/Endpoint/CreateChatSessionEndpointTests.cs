using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Carter;
using Chat.API.Features.CreateChatSession;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Chat.API.Test.Endpoint
{
    public class CreateChatSessionEndpointTests
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;
        private readonly Mock<IChatSessionRepository> _repositoryMock;

        public CreateChatSessionEndpointTests()
        {
            // Thiết lập mock cho repository
            _repositoryMock = new Mock<IChatSessionRepository>();
            _repositoryMock.Setup(r => r.GetChatSessionByUsersAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                           .ReturnsAsync((ChatSession)null);
            _repositoryMock.Setup(r => r.AddChatSessionAsync(It.IsAny<ChatSession>()))
                           .Callback<ChatSession>(cs =>
                           {
                               // Nếu Id đang là Guid.Empty thì gán một Guid mới
                               if (cs.Id == Guid.Empty)
                                   cs.Id = Guid.NewGuid();
                           })
                           .Returns(Task.CompletedTask);

            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton(_repositoryMock.Object);
                    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateChatSessionHandler).Assembly));
                    services.AddCarter();
                    services.AddRouting();
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "Test";
                        options.DefaultChallengeScheme = "Test";
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                    services.AddAuthorization();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication(); // Phải gọi trước UseAuthorization
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
        public async Task CreateChatSession_ReturnsCreated_WhenValidRequest()
        {
            // Arrange
            var user2Id = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var request = new CreateChatSessionRequest(user2Id);
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Thiết lập header token và FakeUser cho authentication thành công
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "dummy-token");
            TestAuthHandler.FakeUser = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "Test")
            );

            // Act
            var response = await _client.PostAsync("/api/chats", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = JsonSerializer.Deserialize<CreateChatSessionResult>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotEqual(Guid.Empty, result.ChatSessionId);
        }

        [Fact]
        public async Task CreateChatSession_ReturnsUnauthorized_WhenNoToken()
        {
            // Arrange
            var request = new CreateChatSessionRequest(Guid.NewGuid());
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Mô phỏng không có token: xóa header và đảm bảo FakeUser là null
            TestAuthHandler.FakeUser = null;
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.PostAsync("/api/chats", content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateChatSession_ReturnsBadRequest_WhenUser2IdIsEmpty()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var request = new CreateChatSessionRequest(Guid.Empty);
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Thiết lập token và FakeUser cho authentication thành công
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "dummy-token");
            TestAuthHandler.FakeUser = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "Test")
            );

            // Act
            var response = await _client.PostAsync("/api/chats", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}