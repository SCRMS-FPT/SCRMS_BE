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
    public class RoleEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public RoleEndpointTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task AssignRoles_ShouldReturnUnauthorized_WhenNotAuthenticated()
        {
            var request = new { UserId = "00000000-0000-0000-0000-000000000000", Roles = new string[] { "Admin" } };

            var response = await _client.PostAsJsonAsync("/api/identity/admin/assign-roles", request);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}