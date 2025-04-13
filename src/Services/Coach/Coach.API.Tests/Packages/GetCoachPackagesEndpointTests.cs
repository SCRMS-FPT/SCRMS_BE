using Coach.API.Features.Packages.GetActivePackages;
using Coach.API.Features.Packages.GetCoachPackages;
using Coach.API.Tests.TestHelpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.IdentityModel.Tokens.Jwt; // Use specific namespace
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Coach.API.Tests.Packages
{
    public class GetCoachPackagesEndpointTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly GetCoachPackagesEndpoint _endpoint;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<ClaimsPrincipal> _mockUser;
        private readonly TestEndpointRouteBuilder _endpointRouteBuilder;

        public GetCoachPackagesEndpointTests()
        {
            _mockSender = new Mock<ISender>();
            _endpoint = new GetCoachPackagesEndpoint();
            _mockHttpContext = new Mock<HttpContext>();
            _mockUser = new Mock<ClaimsPrincipal>();
            _endpointRouteBuilder = new TestEndpointRouteBuilder();

            _mockHttpContext.Setup(x => x.User).Returns(_mockUser.Object);

            // Add routes in constructor
            _endpoint.AddRoutes(_endpointRouteBuilder);
        }

        [Fact]
        public async Task GetCoachPackages_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/me/packages");

            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            var packages = new List<PackageResponse>
            {
                new(Guid.NewGuid(), coachId, "Package 1", "Description 1", 100.0m, 5, "active", DateTime.UtcNow, DateTime.UtcNow),
                new(Guid.NewGuid(), coachId, "Package 2", "Description 2", 200.0m, 10, "active", DateTime.UtcNow, DateTime.UtcNow)
            };

            _mockSender.Setup(x => x.Send(It.IsAny<GetCoachPackagesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(packages);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object) as IResult;

            // Assert
            var okResult = Assert.IsType<Ok<List<PackageResponse>>>(result);
            var response = okResult.Value;

            Assert.NotNull(response);
            Assert.Equal(2, response.Count);

            // Check that sender was called with right query
            _mockSender.Verify(x => x.Send(
                It.Is<GetCoachPackagesQuery>(q => q.CoachId == coachId),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetCoachPackages_NoUserClaim_ReturnsUnauthorized()
        {
            // Arrange
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/me/packages");

            // Set up HttpContext with no user claim
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns((Claim)null);
            _mockUser.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier)).Returns((Claim)null);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object) as IResult;

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);

            // Verify that Send was never called
            _mockSender.Verify(x => x.Send(It.IsAny<GetCoachPackagesQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetCoachPackages_InvalidGuidFormat_ReturnsUnauthorized()
        {
            // Arrange
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/me/packages");

            // Set up HttpContext with invalid GUID format
            var claim = new Claim(JwtRegisteredClaimNames.Sub, "not-a-guid");
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object) as IResult;

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);

            // Verify that Send was never called
            _mockSender.Verify(x => x.Send(It.IsAny<GetCoachPackagesQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetCoachPackages_FallbackToNameIdentifierClaim_ReturnsOkResult()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/me/packages");

            // Set up HttpContext with NameIdentifier claim as fallback
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns((Claim)null);
            var claim = new Claim(ClaimTypes.NameIdentifier, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier)).Returns(claim);

            var packages = new List<PackageResponse>
            {
                new(Guid.NewGuid(), coachId, "Package 1", "Description 1", 100.0m, 5, "active", DateTime.UtcNow, DateTime.UtcNow)
            };

            _mockSender.Setup(x => x.Send(It.IsAny<GetCoachPackagesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(packages);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object) as IResult;

            // Assert
            Assert.IsType<Ok<List<PackageResponse>>>(result);

            // Check that sender was called with right id
            _mockSender.Verify(x => x.Send(
                It.Is<GetCoachPackagesQuery>(q => q.CoachId == coachId),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetCoachPackages_EmptyPackagesList_ReturnsEmptyList()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/me/packages");

            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            // Empty packages list
            var packages = new List<PackageResponse>();

            _mockSender.Setup(x => x.Send(It.IsAny<GetCoachPackagesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(packages);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object) as IResult;

            // Assert
            var okResult = Assert.IsType<Ok<List<PackageResponse>>>(result);
            var response = okResult.Value;

            Assert.NotNull(response);
            Assert.Empty(response);
        }

        [Fact]
        public async Task GetCoachPackages_MediatorThrowsException_PropagatesException()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/me/packages");

            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            // Set up mediator to throw exception
            _mockSender.Setup(x => x.Send(It.IsAny<GetCoachPackagesQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await route.InvokeAsync(_mockHttpContext.Object, _mockSender.Object));

            // Verify that Send was called
            _mockSender.Verify(x => x.Send(It.IsAny<GetCoachPackagesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}