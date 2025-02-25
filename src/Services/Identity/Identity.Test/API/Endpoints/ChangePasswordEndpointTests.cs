using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Test.API.Endpoints
{
    public class ChangePasswordEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ChangePasswordEndpointTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/identity/change-password", new { OldPassword = "old", NewPassword = "new" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}