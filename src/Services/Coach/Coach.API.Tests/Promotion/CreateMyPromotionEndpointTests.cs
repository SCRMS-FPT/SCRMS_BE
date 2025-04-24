using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Coach.API.Features.Promotion.CreateMyPromotion;
using Coach.API.Features.Promotion.CreateCoachPromotion;
using Moq;
using Xunit;
using Coach.API.Tests.TestHelpers;

namespace Coach.API.Tests.Promotion
{
    public class CreateMyPromotionEndpointTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly CreateMyPromotionEndpoint _endpoint;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<ClaimsPrincipal> _mockUser;
        private readonly TestEndpointRouteBuilder _endpointRouteBuilder;

        public CreateMyPromotionEndpointTests()
        {
            _mockSender = new Mock<ISender>();
            _endpoint = new CreateMyPromotionEndpoint();
            _mockHttpContext = new Mock<HttpContext>();
            _mockUser = new Mock<ClaimsPrincipal>();
            _endpointRouteBuilder = new TestEndpointRouteBuilder();

            _mockHttpContext.Setup(x => x.User).Returns(_mockUser.Object);

            // Add routes in constructor
            _endpoint.AddRoutes(_endpointRouteBuilder);
        }

        // Test 1: Valid JWT token and promotion data
        [Fact]
        public async Task CreateMyPromotion_ValidToken_ReturnsOk()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var request = new CreateMyPromotionRequest(
                PackageId: Guid.NewGuid(),
                Description: "Summer Discount",
                DiscountType: "Percentage",
                DiscountValue: 20.0m,
                ValidFrom: DateOnly.FromDateTime(DateTime.Today),
                ValidTo: DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            );

            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/me/promotions");

            _mockSender.Setup(s => s.Send(It.IsAny<CreateCoachPromotionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateCoachPromotionResult(Guid.NewGuid()));

            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, request, _mockSender.Object);

            // Assert
            Assert.IsType<Ok<CreateCoachPromotionResult>>(result);
            _mockSender.Verify(s => s.Send(
                It.Is<CreateCoachPromotionCommand>(cmd =>
                    cmd.CoachId == coachId &&
                    cmd.Description == request.Description),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // Test 2: Invalid JWT token
        [Fact]
        public async Task CreateMyPromotion_InvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var request = new CreateMyPromotionRequest(
                PackageId: Guid.NewGuid(),
                Description: "Summer Discount",
                DiscountType: "Percentage",
                DiscountValue: 20.0m,
                ValidFrom: DateOnly.FromDateTime(DateTime.Today),
                ValidTo: DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            );

            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/me/promotions");

            // Set up HttpContext with no valid user claim
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns((Claim)null);
            _mockUser.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier)).Returns((Claim)null);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, request, _mockSender.Object);

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
            _mockSender.Verify(s => s.Send(It.IsAny<CreateCoachPromotionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}