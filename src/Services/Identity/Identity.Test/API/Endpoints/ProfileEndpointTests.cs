using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Test.API.Endpoints
{
    public class ProfileEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ProfileEndpointTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetProfile_ShouldReturnUnauthorized_WhenNotAuthenticated()
        {
            var response = await _client.GetAsync("/api/identity/get-profile");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}