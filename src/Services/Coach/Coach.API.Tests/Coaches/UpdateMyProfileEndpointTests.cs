using Coach.API.Features.Coaches.UpdateCoach;
using Coach.API.Features.Coaches.UpdateMyProfile;
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

namespace Coach.API.Tests.Coaches
{
    public class UpdateMyProfileEndpointTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly UpdateMyProfileEndpoint _endpoint;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<ClaimsPrincipal> _mockUser;
        private readonly MockFormFile _mockFile;
        private readonly TestEndpointRouteBuilder _endpointRouteBuilder;

        public UpdateMyProfileEndpointTests()
        {
            _mockSender = new Mock<ISender>();
            _endpoint = new UpdateMyProfileEndpoint();
            _mockHttpContext = new Mock<HttpContext>();
            _mockUser = new Mock<ClaimsPrincipal>();
            _mockFile = new MockFormFile();
            _endpointRouteBuilder = new TestEndpointRouteBuilder();

            _mockHttpContext.Setup(x => x.User).Returns(_mockUser.Object);

            // Add routes in constructor
            _endpoint.AddRoutes(_endpointRouteBuilder);
        }

        [Fact]
        public async Task UpdateMyProfile_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/me");

            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            var claims = new List<Claim> { claim };
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            var request = new UpdateCoachRequest
            {
                FullName = "Updated Coach",
                Email = "updated@coach.com",
                Phone = "0987654321",
                Bio = "Updated bio",
                RatePerHour = 75.0m,
                NewAvatar = _mockFile,
                NewImages = new List<IFormFile> { _mockFile },
                ExistingImageUrls = new List<string> { "image1.jpg" },
                ImagesToDelete = new List<string> { "old-image.jpg" },
                ListSport = new List<Guid> { Guid.NewGuid() }
            };

            _mockSender.Setup(x => x.Send(It.IsAny<UpdateCoachCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MediatR.Unit.Value);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, request, _mockSender.Object) as IResult;

            // Assert
            var okResult = Assert.IsType<Ok<object>>(result);
            var response = okResult.Value as dynamic;

            // Check that sender was called with right command
            _mockSender.Verify(x => x.Send(
                It.Is<UpdateCoachCommand>(c =>
                    c.CoachId == coachId &&
                    c.FullName == request.FullName &&
                    c.Email == request.Email &&
                    c.Phone == request.Phone &&
                    c.Bio == request.Bio &&
                    c.RatePerHour == request.RatePerHour
                ),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateMyProfile_NoUserClaim_ReturnsUnauthorized()
        {
            // Arrange
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/me");

            // Set up HttpContext with no user claim
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns((Claim)null);
            _mockUser.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier)).Returns((Claim)null);

            var request = new UpdateCoachRequest
            {
                FullName = "Updated Coach",
                Email = "updated@coach.com"
            };

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, request, _mockSender.Object) as IResult;

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);

            // Verify that Send was never called
            _mockSender.Verify(x => x.Send(It.IsAny<UpdateCoachCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMyProfile_InvalidGuidFormat_ReturnsUnauthorized()
        {
            // Arrange
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/me");

            // Set up HttpContext with invalid GUID format
            var claim = new Claim(JwtRegisteredClaimNames.Sub, "not-a-guid");
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            var request = new UpdateCoachRequest
            {
                FullName = "Updated Coach",
                Email = "updated@coach.com"
            };

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, request, _mockSender.Object) as IResult;

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);

            // Verify that Send was never called
            _mockSender.Verify(x => x.Send(It.IsAny<UpdateCoachCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMyProfile_FallbackToNameIdentifierClaim_ReturnsOkResult()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/me");

            // Set up HttpContext with NameIdentifier claim as fallback
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns((Claim)null);
            var claim = new Claim(ClaimTypes.NameIdentifier, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier)).Returns(claim);

            var request = new UpdateCoachRequest
            {
                FullName = "Updated Coach",
                Email = "updated@coach.com",
                Phone = "0987654321",
                Bio = "Updated bio",
                RatePerHour = 75.0m
            };

            _mockSender.Setup(x => x.Send(It.IsAny<UpdateCoachCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MediatR.Unit.Value);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, request, _mockSender.Object) as IResult;

            // Assert
            Assert.IsType<Ok<object>>(result);

            // Check that sender was called with right id
            _mockSender.Verify(x => x.Send(
                It.Is<UpdateCoachCommand>(c => c.CoachId == coachId),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateMyProfile_WithoutOptionalParameters_ReturnsOkResult()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/me");

            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            // Request without optional parameters
            var request = new UpdateCoachRequest
            {
                FullName = "Updated Coach",
                Email = "updated@coach.com",
                Phone = "0987654321",
                Bio = "Updated bio",
                RatePerHour = 75.0m,
                // No avatar, no images, no sports
            };

            _mockSender.Setup(x => x.Send(It.IsAny<UpdateCoachCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MediatR.Unit.Value);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, request, _mockSender.Object) as IResult;

            // Assert
            Assert.IsType<Ok<object>>(result);

            // Check that sender was called with empty lists for optional parameters
            _mockSender.Verify(x => x.Send(
                It.Is<UpdateCoachCommand>(c =>
                    c.NewAvatarFile == null &&
                    c.NewImageFiles.Count == 0 &&
                    c.ExistingImageUrls.Count == 0 &&
                    c.ImagesToDelete.Count == 0
                ),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}