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
using Coach.API.Features.Promotion.GetAllPromotion;
using Coach.API.Features.Promotion.GetMyPromotions;
using Moq;
using Xunit;

namespace Coach.API.Tests.Promotion
{
    public class GetMyPromotionsEndpointTests
    {
        // Test 1: Valid JWT token
        [Fact]
        public async Task GetMyPromotions_ValidToken_ReturnsOk()
        {
            // Arrange
            var coachId = Guid.NewGuid();

            var promotions = new List<PromotionRecord>
            {
                new PromotionRecord(
                    Id: Guid.NewGuid(),
                    Description: "Summer Discount",
                    DiscountType: "Percentage",
                    DiscountValue: 20.0m,
                    ValidFrom: DateOnly.FromDateTime(DateTime.Today),
                    ValidTo: DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
                    PackageId: Guid.NewGuid(),
                    PackageName: "Premium Package",
                    CreatedAt: DateTime.Now,
                    UpdatedAt: DateTime.Now
                ),
                new PromotionRecord(
                    Id: Guid.NewGuid(),
                    Description: "Winter Special",
                    DiscountType: "Fixed",
                    DiscountValue: 50.0m,
                    ValidFrom: DateOnly.FromDateTime(DateTime.Today),
                    ValidTo: DateOnly.FromDateTime(DateTime.Today.AddDays(60)),
                    PackageId: null,
                    PackageName: null,
                    CreatedAt: DateTime.Now,
                    UpdatedAt: DateTime.Now
                )
            };

            var mockSender = new Mock<ISender>();
            mockSender.Setup(s => s.Send(It.IsAny<GetAllPromotionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(promotions);

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString())
            }));

            // Act
            var endpoint = new GetMyPromotionsEndpoint();
            var mockEndpointRouteBuilder = new Mock<IEndpointRouteBuilder>();

            Func<ISender, HttpContext, int, int, Task<IResult>> capturedHandler = null;

            mockEndpointRouteBuilder
                .Setup(erb => erb.MapGet(It.IsAny<string>(), It.IsAny<Delegate>()))
                .Callback<string, Delegate>((pattern, handler) =>
                {
                    capturedHandler = (Func<ISender, HttpContext, int, int, Task<IResult>>)handler;
                })
                .Returns(Mock.Of<RouteHandlerBuilder>());

            endpoint.AddRoutes(mockEndpointRouteBuilder.Object);

            // Call the handler directly
            var result = await capturedHandler(mockSender.Object, httpContext, 1, 10);

            // Assert
            Assert.IsType<Ok<List<PromotionRecord>>>(result);
            mockSender.Verify(s => s.Send(
                It.Is<GetAllPromotionQuery>(q =>
                    q.CoachId == coachId &&
                    q.Page == 1 &&
                    q.RecordPerPage == 10),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // Test 2: Invalid JWT token
        [Fact]
        public async Task GetMyPromotions_InvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var mockSender = new Mock<ISender>();
            var httpContext = new DefaultHttpContext(); // No claims

            // Act
            var endpoint = new GetMyPromotionsEndpoint();
            var mockEndpointRouteBuilder = new Mock<IEndpointRouteBuilder>();

            Func<ISender, HttpContext, int, int, Task<IResult>> capturedHandler = null;

            mockEndpointRouteBuilder
                .Setup(erb => erb.MapGet(It.IsAny<string>(), It.IsAny<Delegate>()))
                .Callback<string, Delegate>((pattern, handler) =>
                {
                    capturedHandler = (Func<ISender, HttpContext, int, int, Task<IResult>>)handler;
                })
                .Returns(Mock.Of<RouteHandlerBuilder>());

            endpoint.AddRoutes(mockEndpointRouteBuilder.Object);

            // Call the handler directly
            var result = await capturedHandler(mockSender.Object, httpContext, 1, 10);

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
            mockSender.Verify(s => s.Send(It.IsAny<GetAllPromotionQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // Test 3: Custom pagination parameters
        [Fact]
        public async Task GetMyPromotions_CustomPagination_UsesCorrectParameters()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            const int page = 2;
            const int recordsPerPage = 20;

            var mockSender = new Mock<ISender>();
            mockSender.Setup(s => s.Send(It.IsAny<GetAllPromotionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PromotionRecord>());

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString())
            }));

            // Act
            var endpoint = new GetMyPromotionsEndpoint();
            var mockEndpointRouteBuilder = new Mock<IEndpointRouteBuilder>();

            Func<ISender, HttpContext, int, int, Task<IResult>> capturedHandler = null;

            mockEndpointRouteBuilder
                .Setup(erb => erb.MapGet(It.IsAny<string>(), It.IsAny<Delegate>()))
                .Callback<string, Delegate>((pattern, handler) =>
                {
                    capturedHandler = (Func<ISender, HttpContext, int, int, Task<IResult>>)handler;
                })
                .Returns(Mock.Of<RouteHandlerBuilder>());

            endpoint.AddRoutes(mockEndpointRouteBuilder.Object);

            // Call the handler directly
            var result = await capturedHandler(mockSender.Object, httpContext, page, recordsPerPage);

            // Assert
            Assert.IsType<Ok<List<PromotionRecord>>>(result);
            mockSender.Verify(s => s.Send(
                It.Is<GetAllPromotionQuery>(q =>
                    q.CoachId == coachId &&
                    q.Page == page &&
                    q.RecordPerPage == recordsPerPage),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}