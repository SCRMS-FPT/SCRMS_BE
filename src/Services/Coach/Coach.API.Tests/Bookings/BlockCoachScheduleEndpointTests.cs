using Coach.API.Features.Bookings.BlockCoachSchedule;
using Coach.API.Tests.TestHelpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Coach.API.Tests.Bookings
{
    public class BlockCoachScheduleEndpointTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly BlockCoachScheduleEndpoint _endpoint;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<ClaimsPrincipal> _mockUser;
        private readonly Mock<ILogger<BlockCoachScheduleEndpoint>> _mockLogger;
        private readonly TestEndpointRouteBuilder _endpointRouteBuilder;

        public BlockCoachScheduleEndpointTests()
        {
            _mockSender = new Mock<ISender>();
            _mockLogger = new Mock<ILogger<BlockCoachScheduleEndpoint>>();
            _endpoint = new BlockCoachScheduleEndpoint(_mockLogger.Object);
            _mockHttpContext = new Mock<HttpContext>();
            _mockUser = new Mock<ClaimsPrincipal>();
            _endpointRouteBuilder = new TestEndpointRouteBuilder();

            _mockHttpContext.Setup(x => x.User).Returns(_mockUser.Object);

            // Add routes in constructor
            _endpoint.AddRoutes(_endpointRouteBuilder);
        }

        [Fact]
        public async Task BlockCoachSchedule_ValidRequest_ReturnsCreatedResult()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/block-schedule");

            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            var sportId = Guid.NewGuid();
            var blockDate = DateTime.Today.AddDays(1);
            var startTime = DateTime.Today.AddHours(10);
            var endTime = DateTime.Today.AddHours(12);

            var request = new BlockCoachScheduleRequest(
                SportId: sportId,
                BlockDate: blockDate,
                StartTime: startTime,
                EndTime: endTime,
                Notes: "Personal appointment"
            );

            var blockId = Guid.NewGuid();
            var commandResponse = new BlockCoachScheduleResult(blockId);

            _mockSender.Setup(x => x.Send(It.IsAny<BlockCoachScheduleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(commandResponse);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, request, _mockSender.Object) as IResult;

            // Assert
            var createdResult = Assert.IsType<Created<BlockCoachScheduleResult>>(result);
            var response = createdResult.Value;

            Assert.NotNull(response);
            Assert.Equal(blockId, response.BookingId);

            // Check that sender was called with right command
            _mockSender.Verify(x => x.Send(
                It.Is<BlockCoachScheduleCommand>(c =>
                    c.CoachId == coachId &&
                    c.SportId == sportId),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task BlockCoachSchedule_NoUserClaim_ReturnsUnauthorized()
        {
            // Arrange
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/block-schedule");

            // Set up HttpContext with no user claim
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns((Claim)null);
            _mockUser.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier)).Returns((Claim)null);

            var sportId = Guid.NewGuid();
            var blockDate = DateTime.Today.AddDays(1);
            var startTime = DateTime.Today.AddHours(10);
            var endTime = DateTime.Today.AddHours(12);

            var request = new BlockCoachScheduleRequest(
                SportId: sportId,
                BlockDate: blockDate,
                StartTime: startTime,
                EndTime: endTime,
                Notes: "Personal appointment"
            );

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, request, _mockSender.Object) as IResult;

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);

            // Verify that Send was never called
            _mockSender.Verify(x => x.Send(It.IsAny<BlockCoachScheduleCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task BlockCoachSchedule_InvalidGuidFormat_ReturnsUnauthorized()
        {
            // Arrange
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/block-schedule");

            // Set up HttpContext with invalid GUID format
            var claim = new Claim(JwtRegisteredClaimNames.Sub, "not-a-guid");
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);

            var sportId = Guid.NewGuid();
            var blockDate = DateTime.Today.AddDays(1);
            var startTime = DateTime.Today.AddHours(10);
            var endTime = DateTime.Today.AddHours(12);

            var request = new BlockCoachScheduleRequest(
                SportId: sportId,
                BlockDate: blockDate,
                StartTime: startTime,
                EndTime: endTime,
                Notes: "Personal appointment"
            );

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, request, _mockSender.Object) as IResult;

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);

            // Verify that Send was never called
            _mockSender.Verify(x => x.Send(It.IsAny<BlockCoachScheduleCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task BlockCoachSchedule_FallbackToNameIdentifierClaim_ReturnsCreatedResult()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var route = _endpointRouteBuilder.GetRouteByPattern("/coaches/block-schedule");

            // Set up HttpContext with NameIdentifier claim as fallback
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns((Claim)null);
            var claim = new Claim(ClaimTypes.NameIdentifier, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier)).Returns(claim);

            var sportId = Guid.NewGuid();
            var blockDate = DateTime.Today.AddDays(1);
            var startTime = DateTime.Today.AddHours(10);
            var endTime = DateTime.Today.AddHours(12);

            var request = new BlockCoachScheduleRequest(
                SportId: sportId,
                BlockDate: blockDate,
                StartTime: startTime,
                EndTime: endTime,
                Notes: "Personal appointment"
            );

            var blockId = Guid.NewGuid();
            var commandResponse = new BlockCoachScheduleResult(blockId);

            _mockSender.Setup(x => x.Send(It.IsAny<BlockCoachScheduleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(commandResponse);

            // Act
            var result = await route.InvokeAsync(_mockHttpContext.Object, request, _mockSender.Object) as IResult;

            // Assert
            var createdResult = Assert.IsType<Created<BlockCoachScheduleResult>>(result);
            var response = createdResult.Value;

            Assert.NotNull(response);
            Assert.Equal(blockId, response.BookingId);

            // Check that sender was called with right id
            _mockSender.Verify(x => x.Send(
                It.Is<BlockCoachScheduleCommand>(c => c.CoachId == coachId),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}