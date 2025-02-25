using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Test.API.Endpoints
{
    public class UserManagementEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public UserManagementEndpointTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetUsers_ShouldReturnUnauthorized_WhenNotAuthenticated()
        {
            var response = await _client.GetAsync("/api/users/");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}