using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Identity.Test.API.Endpoints
{
    public class LoginEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public LoginEndpointTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WithToken()
        {
            // Giả sử trong DB test đã có user admin được seed
            var request = new { Email = "admin@gmail.com", Password = "Admin123!" };

            var response = await _client.PostAsJsonAsync("/api/identity/login", request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.TryGetProperty("token", out var tokenProperty).Should().BeTrue();
            string token = tokenProperty.GetString();
            token.Should().NotBeNullOrEmpty();
        }
    }
}