using Carter;
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

        public GetChatSessionsEndpointTests()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    var sessionRepoMock = new Mock<IChatSessionRepository>();
                    sessionRepoMock.Setup(r => r.GetChatSessionByUserIdAsync(It.IsAny<Guid>()))
                                   .ReturnsAsync(new List<ChatSessionResponse>());
                    services.AddSingleton(sessionRepoMock.Object);
                    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetChatSessionsHandler).Assembly));
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
        public async Task GetChatSessions_ReturnsOk_WhenValidRequest()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "Bearer token");
            // Thiết lập FakeUser với một giá trị GUID hợp lệ cho NameIdentifier.
            TestAuthHandler.FakeUser = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }, "Test"));

            // Act: Truyền rõ query string để model binding hoạt động (page=1, limit=10)
            var response = await _client.GetAsync("/api/chats?page=1&limit=10");

            // Assert: Dự kiến trả về OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = JsonSerializer.Deserialize<List<ChatSessionResponse>>(await response.Content.ReadAsStringAsync());
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetChatSessions_ReturnsUnauthorized_WhenNoToken()
        {
            // Arrange: Không thiết lập FakeUser và header Authorization
            TestAuthHandler.FakeUser = null;
            _client.DefaultRequestHeaders.Authorization = null;

            // Act: Gọi API với query string rõ ràng
            var response = await _client.GetAsync("/api/chats?page=1&limit=10");

            // Assert: Dự kiến trả về Unauthorized (401)
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetChatSessions_ReturnsBadRequest_WhenLimitIsZero()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "Bearer token");
            TestAuthHandler.FakeUser = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }, "Test"));

            // Act: Truyền limit=0 (với page=1) để kích hoạt validation trả về BadRequest
            var response = await _client.GetAsync("/api/chats?page=1&limit=0");

            // Assert: Dự kiến trả về BadRequest (400)
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}