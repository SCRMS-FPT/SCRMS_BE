using Coach.API.Features.Coaches.GetCoaches;
using Coach.API.Features.Coaches.GetMyCoachProfile;
using Coach.API.Tests.TestHelpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.IdentityModel.Tokens.Jwt;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Coach.API.Tests.Coaches
{
    public class GetMyCoachProfileEndpointTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly GetMyCoachProfileEndpoint _endpoint;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<ClaimsPrincipal> _mockUser;
        private readonly TestEndpointRouteBuilder _endpointRouteBuilder;

        public GetMyCoachProfileEndpointTests()
        {
            _mockSender = new Mock<ISender>();
            _endpoint = new GetMyCoachProfileEndpoint();
            _mockHttpContext = new Mock<HttpContext>();
            _mockUser = new Mock<ClaimsPrincipal>();
            _endpointRouteBuilder = new TestEndpointRouteBuilder();

            _mockHttpContext.Setup(x => x.User).Returns(_mockUser.Object);

            // Add routes before each test
            _endpoint.AddRoutes(_endpointRouteBuilder);
        }

        [Fact]
        public async Task GetMyCoachProfile_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var coachId = Guid.NewGuid();

            // Get the first route (the GET /coaches/me endpoint)
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/me");

            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            var coachResponse = new CoachResponse(
                coachId,
                "Test Coach",
                "coach@test.com",
                "1234567890",
                "http://test.com/avatar.jpg",
                new List<string> { "image1.jpg", "image2.jpg" },
                new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
                "Coach bio",
                50.0m,
                DateTime.UtcNow,
                new List<CoachPackageResponse>(),
                new WeeklySchedule());

            _mockSender.Setup(x => x.Send(It.IsAny<GetMyCoachProfileQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(coachResponse);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object) as IResult;

            // Assert
            var okResult = Assert.IsType<Ok<CoachResponse>>(result);
            var response = okResult.Value;

            Assert.Equal(coachId, response.Id);
            Assert.Equal("Test Coach", response.FullName);
            Assert.Equal("coach@test.com", response.Email);

            // Check that sender was called with right query
            _mockSender.Verify(x => x.Send(
                It.Is<GetMyCoachProfileQuery>(q => q.UserId == coachId),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetMyCoachProfile_NoUserClaim_ReturnsUnauthorized()
        {
            // Arrange
            // Get the first route (the GET /coaches/me endpoint)
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/me");

            // Set up HttpContext with no user claim
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns((Claim)null);
            _mockUser.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier)).Returns((Claim)null);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object) as IResult;

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);

            // Verify that Send was never called
            _mockSender.Verify(x => x.Send(It.IsAny<GetMyCoachProfileQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetMyCoachProfile_InvalidGuidFormat_ReturnsUnauthorized()
        {
            // Arrange
            // Get the first route (the GET /coaches/me endpoint)
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/me");

            // Set up HttpContext with invalid GUID format
            var claim = new Claim(JwtRegisteredClaimNames.Sub, "not-a-guid");
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object) as IResult;

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);

            // Verify that Send was never called
            _mockSender.Verify(x => x.Send(It.IsAny<GetMyCoachProfileQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetMyCoachProfile_FallbackToNameIdentifierClaim_ReturnsOkResult()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            // Get the first route (the GET /coaches/me endpoint)
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/me");

            // Set up HttpContext with NameIdentifier claim as fallback
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns((Claim)null);
            var claim = new Claim(ClaimTypes.NameIdentifier, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier)).Returns(claim);

            var coachResponse = new CoachResponse(
                coachId,
                "Test Coach",
                "coach@test.com",
                "1234567890",
                "http://test.com/avatar.jpg",
                new List<string>(),
                new List<Guid>(),
                "Coach bio",
                50.0m,
                DateTime.UtcNow,
                new List<CoachPackageResponse>(),
                new WeeklySchedule());

            _mockSender.Setup(x => x.Send(It.IsAny<GetMyCoachProfileQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(coachResponse);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object) as IResult;

            // Assert
            Assert.IsType<Ok<CoachResponse>>(result);

            // Check that sender was called with right id
            _mockSender.Verify(x => x.Send(
                It.Is<GetMyCoachProfileQuery>(q => q.UserId == coachId),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetMyCoachProfile_MediatorThrowsException_PropagatesException()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            // Get the first route (the GET /coaches/me endpoint)
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/me");

            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            // Set up mediator to throw exception
            _mockSender.Setup(x => x.Send(It.IsAny<GetMyCoachProfileQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object));

            // Verify that Send was called
            _mockSender.Verify(x => x.Send(It.IsAny<GetMyCoachProfileQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}