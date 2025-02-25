using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Test.API.Endpoints
{
    public class ServicePackagesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ServicePackagesEndpointTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetServicePackages_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/service-packages/");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetServicePackageById_ShouldReturnNotFound_WhenIdDoesNotExist()
        {
            var response = await _client.GetAsync($"/api/service-packages/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}