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
using Microsoft.AspNetCore.Routing;

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

            // Clear any existing routes and add routes specifically for this test
            _endpointRouteBuilder = new TestEndpointRouteBuilder();
            _endpoint.AddRoutes(_endpointRouteBuilder);
        }

        [Fact]
        public async Task GetMyPromotions_ValidToken_ReturnsOk()
        {
            // Arrange
            var coachId = Guid.NewGuid();

            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            // Set up mock response
            var promotions = new List<PromotionRecord>();
            _mockSender.Setup(x => x.Send(
                It.Is<GetAllPromotionQuery>(q => q.CoachId == coachId && q.Page == 1 && q.RecordPerPage == 10),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(promotions);

            // Act - Use the specific GET endpoint that your test is targeting
            var result = await _endpointRouteBuilder.GetRouteByPattern("/api/promotions")
                .InvokeAsync(_mockHttpContext.Object, _mockSender.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IResult>(result);

            // Check that sender was called with right query
            _mockSender.Verify(x => x.Send(
                It.Is<GetAllPromotionQuery>(q => q.CoachId == coachId && q.Page == 1 && q.RecordPerPage == 10),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetMyPromotions_NoUserClaim_ReturnsUnauthorized()
        {
            // Arrange
            var route = _endpointRouteBuilder.GetRouteByPattern("/api/promotions");

            // Set up HttpContext with no user claim
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns((Claim)null);
            _mockUser.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier)).Returns((Claim)null);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object);

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);

            // Verify that Send was never called
            _mockSender.Verify(x => x.Send(It.IsAny<GetAllPromotionQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetMyPromotions_InvalidGuidFormat_ReturnsUnauthorized()
        {
            // Arrange
            var route = _endpointRouteBuilder.GetRouteByPattern("/api/promotions");

            // Set up HttpContext with invalid GUID format
            var claim = new Claim(JwtRegisteredClaimNames.Sub, "not-a-guid");
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object);

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);

            // Verify that Send was never called
            _mockSender.Verify(x => x.Send(It.IsAny<GetAllPromotionQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetMyPromotions_CustomPagination_UsesCorrectParameters()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var route = _endpointRouteBuilder.GetRouteByPattern("/api/promotions");

            // Create a request with custom pagination parameters
            var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "Page", new StringValues("2") },
                { "RecordPerPage", new StringValues("5") }
            });
            _mockHttpContext.Setup(x => x.Request.Query).Returns(queryCollection);

            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            // Empty list of promotions
            var promotions = new List<PromotionRecord>();

            // Setup the mock for the specific query parameters
            _mockSender
                .Setup(x => x.Send(
                    It.Is<GetAllPromotionQuery>(q =>
                        q.CoachId == coachId &&
                        q.Page == 2 &&
                        q.RecordPerPage == 5),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(promotions);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IResult>(result);

            // Verify sender was called with correct pagination parameters
            _mockSender.Verify(x => x.Send(
                It.Is<GetAllPromotionQuery>(q =>
                    q.CoachId == coachId &&
                    q.Page == 2 &&
                    q.RecordPerPage == 5),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetMyPromotions_MediatorThrowsException_PropagatesException()
        {
            // Arrange
            var coachId = Guid.NewGuid();

            // Important: Register the route directly
            _endpoint.AddRoutes(_endpointRouteBuilder);

            // Set up default query parameters
            var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "Page", new StringValues("1") },
                { "RecordPerPage", new StringValues("10") }
            });
            _mockHttpContext.Setup(x => x.Request.Query).Returns(queryCollection);

            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            // Set up mediator to throw exception
            var exception = new Exception("Test exception");
            _mockSender
                .Setup(x => x.Send(
                    It.Is<GetAllPromotionQuery>(q => q.CoachId == coachId),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () =>
                await TestEndpointHelpers.InvokeRouteByPattern(_endpointRouteBuilder, "/api/promotions", _mockHttpContext.Object, _mockSender.Object));

            Assert.Equal("Test exception", ex.Message);
        }
    }
}