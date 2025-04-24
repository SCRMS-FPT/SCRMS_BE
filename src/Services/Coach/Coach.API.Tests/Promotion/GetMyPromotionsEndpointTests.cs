using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using Coach.API.Features.Promotion.GetMyPromotions;
using Coach.API.Features.Promotion.GetAllPromotion;
using Coach.API.Tests.TestHelpers;
using Moq;
using Xunit;
using MediatR;

namespace Coach.API.Tests.Promotion
{
    public class GetMyPromotionsEndpointTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly GetMyPromotionsEndpoint _endpoint;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<ClaimsPrincipal> _mockUser;
        private readonly TestEndpointRouteBuilder _endpointRouteBuilder;

        public GetMyPromotionsEndpointTests()
        {
            _mockSender = new Mock<ISender>();
            _endpoint = new GetMyPromotionsEndpoint();
            _mockHttpContext = new Mock<HttpContext>();
            _mockUser = new Mock<ClaimsPrincipal>();
            _endpointRouteBuilder = new TestEndpointRouteBuilder();

            _mockHttpContext.Setup(x => x.User).Returns(_mockUser.Object);

            // Set up a default mock request
            var request = new Mock<HttpRequest>();
            var defaultQueryCollection = new Dictionary<string, StringValues>();
            request.Setup(r => r.Query).Returns(new QueryCollection(defaultQueryCollection));
            _mockHttpContext.Setup(c => c.Request).Returns(request.Object);

            // Add routes for this specific endpoint test
            // This is important - we need to add our own route here, not rely on pre-registered routes
            _endpoint.AddRoutes(_endpointRouteBuilder);
        }

        [Fact]
        public async Task GetMyPromotions_NoUserClaim_ReturnsUnauthorized()
        {
            // Arrange
            // Use the exact route pattern defined in GetMyPromotionsEndpoint
            var route = _endpointRouteBuilder.GetRouteByPattern("/api/promotions");

            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns((Claim)null);
            _mockUser.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier)).Returns((Claim)null);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object);

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
            _mockSender.Verify(x => x.Send(It.IsAny<GetAllPromotionQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetMyPromotions_InvalidGuidFormat_ReturnsUnauthorized()
        {
            // Arrange
            var route = _endpointRouteBuilder.GetRouteByPattern("/api/promotions");

            var claim = new Claim(JwtRegisteredClaimNames.Sub, "not-a-guid");
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object);

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
            _mockSender.Verify(x => x.Send(It.IsAny<GetAllPromotionQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}