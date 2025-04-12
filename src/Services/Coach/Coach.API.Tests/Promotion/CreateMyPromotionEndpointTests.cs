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

namespace Coach.API.Tests.Promotion
{
    public class CreateMyPromotionEndpointTests
    {
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

            var mockSender = new Mock<ISender>();
            mockSender.Setup(s => s.Send(It.IsAny<CreateCoachPromotionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateCoachPromotionResult(Guid.NewGuid()));

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString())
            }));

            // Act
            var endpoint = new CreateMyPromotionEndpoint();
            var mockEndpointRouteBuilder = new Mock<IEndpointRouteBuilder>();

            Func<CreateMyPromotionRequest, ISender, HttpContext, Task<IResult>> capturedHandler = null;

            mockEndpointRouteBuilder
                .Setup(erb => erb.MapPost(It.IsAny<string>(), It.IsAny<Delegate>()))
                .Callback<string, Delegate>((pattern, handler) =>
                {
                    capturedHandler = (Func<CreateMyPromotionRequest, ISender, HttpContext, Task<IResult>>)handler;
                })
                .Returns(Mock.Of<RouteHandlerBuilder>());

            endpoint.AddRoutes(mockEndpointRouteBuilder.Object);

            // Call the handler directly
            var result = await capturedHandler(request, mockSender.Object, httpContext);

            // Assert
            Assert.IsType<Ok<CreateCoachPromotionResult>>(result);
            mockSender.Verify(s => s.Send(
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

            var mockSender = new Mock<ISender>();
            var httpContext = new DefaultHttpContext(); // No claims

            // Act
            var endpoint = new CreateMyPromotionEndpoint();
            var mockEndpointRouteBuilder = new Mock<IEndpointRouteBuilder>();

            Func<CreateMyPromotionRequest, ISender, HttpContext, Task<IResult>> capturedHandler = null;

            mockEndpointRouteBuilder
                .Setup(erb => erb.MapPost(It.IsAny<string>(), It.IsAny<Delegate>()))
                .Callback<string, Delegate>((pattern, handler) =>
                {
                    capturedHandler = (Func<CreateMyPromotionRequest, ISender, HttpContext, Task<IResult>>)handler;
                })
                .Returns(Mock.Of<RouteHandlerBuilder>());

            endpoint.AddRoutes(mockEndpointRouteBuilder.Object);

            // Call the handler directly
            var result = await capturedHandler(request, mockSender.Object, httpContext);

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
            mockSender.Verify(s => s.Send(It.IsAny<CreateCoachPromotionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}