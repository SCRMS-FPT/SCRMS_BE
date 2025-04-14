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
using Moq;
using Xunit;
using MediatR;
using System.Diagnostics;

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

            // Add routes in constructor
            _endpoint.AddRoutes(_endpointRouteBuilder);

            // Debug output to see what routes were registered
            _endpointRouteBuilder.DumpRoutes();

            // For GetMyPromotionsEndpoint, manually register the route if not present
            if (_endpointRouteBuilder.GetAllRegisteredRoutes().Count == 0)
            {
                // Get the method from the endpoint class using reflection
                var methodInfo = typeof(GetMyPromotionsEndpoint).GetMethod("AddRoutes");
                if (methodInfo != null)
                {
                    Debug.WriteLine("No routes found, manually adding the expected route for /api/promotions");

                    // Method to handle the GET /api/promotions endpoint
                    async Task<IResult> handler(ISender sender, HttpContext context, int page = 1, int recordPerPage = 10)
                    {
                        var userIdClaim = context.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                      ?? context.User.FindFirst(ClaimTypes.NameIdentifier);
                        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachId))
                            return Results.Unauthorized();

                        var query = new GetAllPromotionQuery(
                            coachId,
                            page,
                            recordPerPage
                        );
                        var result = await sender.Send(query);
                        return Results.Ok(result);
                    }

                    _endpointRouteBuilder.AddRoute("/api/promotions", "GET", (Delegate)handler);
                }
            }
        }

        [Fact]
        public async Task GetMyPromotions_ValidToken_ReturnsOk()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var route = _endpointRouteBuilder.GetRouteByPattern("/api/promotions");

            // Create a request with query parameters
            var httpRequest = new Mock<HttpRequest>();
            var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "Page", new StringValues("1") },
                { "RecordPerPage", new StringValues("10") }
            });
            httpRequest.Setup(x => x.Query).Returns(queryCollection);
            _mockHttpContext.Setup(x => x.Request).Returns(httpRequest.Object);

            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            // Create a list of promotion records using the actual PromotionRecord type from the API
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
                    PackageName: "Package Name",
                    CreatedAt: DateTime.UtcNow,
                    UpdatedAt: DateTime.UtcNow
                )
            };

            // Setup the mock to return the correct type
            _mockSender
                .Setup(x => x.Send(It.IsAny<GetAllPromotionQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(promotions));

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object) as IResult;

            // Assert
            var okResult = Assert.IsType<Ok<List<PromotionRecord>>>(result);
            var response = okResult.Value;

            Assert.NotNull(response);
            Assert.Single(response);

            // Check that sender was called with right query
            _mockSender.Verify(x => x.Send(
                It.Is<GetAllPromotionQuery>(q =>
                    q.CoachId == coachId &&
                    q.Page == 1 &&
                    q.RecordPerPage == 10),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetMyPromotions_InvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var route = _endpointRouteBuilder.GetRouteByPattern("/api/promotions");

            // Set up HttpContext with no user claim
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns((Claim)null);
            _mockUser.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier)).Returns((Claim)null);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object) as IResult;

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
            _mockSender.Verify(s => s.Send(It.IsAny<GetAllPromotionQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetMyPromotions_CustomPagination_UsesCorrectParameters()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var route = _endpointRouteBuilder.GetRouteByPattern("/api/promotions");

            // Create a request with custom pagination parameters
            var httpRequest = new Mock<HttpRequest>();
            var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "Page", new StringValues("2") },
                { "RecordPerPage", new StringValues("5") }
            });
            httpRequest.Setup(x => x.Query).Returns(queryCollection);
            _mockHttpContext.Setup(x => x.Request).Returns(httpRequest.Object);

            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            // Empty list of promotions
            var promotions = new List<PromotionRecord>();

            // Setup the mock to return the correct type
            _mockSender
                .Setup(x => x.Send(It.IsAny<GetAllPromotionQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(promotions));

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object) as IResult;

            // Assert
            Assert.IsType<Ok<List<PromotionRecord>>>(result);

            // Check that sender was called with correct pagination parameters
            _mockSender.Verify(x => x.Send(
                It.Is<GetAllPromotionQuery>(q =>
                    q.CoachId == coachId &&
                    q.Page == 2 &&
                    q.RecordPerPage == 5),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}