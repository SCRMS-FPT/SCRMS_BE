using CourtBooking.API.Endpoints;
using CourtBooking.Application.DTOs;
using CourtBooking.Application.SportManagement.Commands.CreateSport;
using CourtBooking.Application.SportManagement.Commands.DeleteSport;
using CourtBooking.Application.SportManagement.Commands.UpdateSport;
using CourtBooking.Application.SportManagement.Queries.GetSportById;
using CourtBooking.Application.SportManagement.Queries.GetSports;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using System.Security.Claims;
using Xunit;

namespace CourtBooking.Test.API.Endpoints
{
    public class SportEndpointsTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly SportEndpoints _endpoints;
        private readonly Mock<HttpContext> _mockHttpContext;

        public SportEndpointsTests()
        {
            _mockSender = new Mock<ISender>();
            _endpoints = new SportEndpoints();
            _mockHttpContext = new Mock<HttpContext>();
        }

        [Fact]
        public async Task GetSports_Should_ReturnOk_When_Called()
        {
            // Arrange
            var sports = new List<SportDTO>
            {
                new SportDTO( Guid.NewGuid(),
                     "Tennis",
                    "Tennis is a racket sport",
                     "tennis.png"),
                new SportDTO(Guid.NewGuid(), "Football","Football is a team sport","football.png")
            };

            var sportsResult = new GetSportsResult(sports);

            _mockSender.Setup(x => x.Send(It.IsAny<GetSportsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sportsResult);

            // Act
            var result = await InvokeGetSportsDelegate();

            // Assert
            Assert.IsType<Ok<GetSportsResult>>(result);
            var okResult = (Ok<GetSportsResult>)result;
            Assert.Equal(2, okResult.Value.Sports.Count);
            Assert.Equal("Tennis", okResult.Value.Sports[0].Name);
            Assert.Equal("Football", okResult.Value.Sports[1].Name);

            _mockSender.Verify(x => x.Send(It.IsAny<GetSportsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetSportById_Should_ReturnOk_When_Exists()
        {
            // Arrange
            var sportId = Guid.NewGuid();

            var sportDto = new SportDTO(
                sportId,
                "Tennis",
                "Tennis is a racket sport",
                 "tennis.png");

            _mockSender.Setup(x => x.Send(It.IsAny<GetSportByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sportDto);

            // Act
            var result = await InvokeGetSportByIdDelegate(sportId);

            // Assert
            Assert.IsType<Ok<SportDTO>>(result);
            var okResult = (Ok<SportDTO>)result;
            Assert.Equal(sportId, okResult.Value.Id);
            Assert.Equal("Tennis", okResult.Value.Name);

            _mockSender.Verify(x => x.Send(
                It.Is<GetSportByIdQuery>(q => q.SportId == sportId),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetSportById_Should_ReturnNotFound_When_NotExists()
        {
            // Arrange
            var sportId = Guid.NewGuid();

            _mockSender.Setup(x => x.Send(It.IsAny<GetSportByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SportDTO)null);

            // Act
            var result = await InvokeGetSportByIdDelegate(sportId);

            // Assert
            Assert.IsType<NotFound>(result);

            _mockSender.Verify(x => x.Send(
                It.Is<GetSportByIdQuery>(q => q.SportId == sportId),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateSport_Should_ReturnCreated_When_Admin()
        {
            // Arrange
            var sportId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims, "TestAuth"));

            _mockHttpContext.Setup(c => c.User).Returns(user);

            var request = new CreateSportRequest("Tennis", "Tennis is a racket sport", "tennis.png");

            _mockSender.Setup(x => x.Send(It.IsAny<CreateSportCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateSportResult(sportId));

            // Act
            var result = await InvokeCreateSportDelegate(request, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Created<CreateSportResult>>(result);
            var created = (Created<CreateSportResult>)result;
            Assert.Equal($"/api/sports/{sportId}", created.Location);
            Assert.Equal(sportId, created.Value.Id);

            _mockSender.Verify(x => x.Send(
                It.Is<CreateSportCommand>(c =>
                    c.Name == "Tennis" &&
                    c.Description == "Tennis is a racket sport" &&
                    c.Icon == "tennis.png"),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateSport_Should_ReturnForbidden_When_NotAdmin()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "User") // Not an admin
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims, "TestAuth"));

            _mockHttpContext.Setup(c => c.User).Returns(user);

            var request = new CreateSportRequest("Tennis", "Tennis is a racket sport", "tennis.png");

            // Act
            var result = await InvokeCreateSportDelegate(request, _mockHttpContext.Object);

            // Assert
            Assert.IsType<ForbidHttpResult>(result);

            _mockSender.Verify(x => x.Send(It.IsAny<CreateSportCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateSport_Should_ReturnOk_When_Admin()
        {
            // Arrange
            var sportId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims, "TestAuth"));

            _mockHttpContext.Setup(c => c.User).Returns(user);

            var request = new UpdateSportRequest(sportId, "Updated Tennis", "Updated tennis description", "updated-tennis.png");

            _mockSender.Setup(x => x.Send(It.IsAny<UpdateSportCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateSportResult(true));

            // Act
            var result = await InvokeUpdateSportDelegate(sportId, request, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Ok<UpdateSportResult>>(result);
            var okResult = (Ok<UpdateSportResult>)result;
            Assert.True(okResult.Value.IsSuccess);

            _mockSender.Verify(x => x.Send(
                It.Is<UpdateSportCommand>(c =>
                    c.Id == sportId &&
                    c.Name == "Updated Tennis" &&
                    c.Description == "Updated tennis description" &&
                    c.Icon == "updated-tennis.png"),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSport_Should_ReturnOk_When_Admin()
        {
            // Arrange
            var sportId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims, "TestAuth"));

            _mockHttpContext.Setup(c => c.User).Returns(user);

            _mockSender.Setup(x => x.Send(It.IsAny<DeleteSportCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteSportResult(true, "Sport deleted successfully"));

            // Act
            var result = await InvokeDeleteSportDelegate(sportId, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Ok<DeleteSportResult>>(result);
            var okResult = (Ok<DeleteSportResult>)result;
            Assert.True(okResult.Value.IsSuccess);
            Assert.Equal("Sport deleted successfully", okResult.Value.Message);

            _mockSender.Verify(x => x.Send(
                It.Is<DeleteSportCommand>(c => c.SportId == sportId),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSport_Should_ReturnForbidden_When_NotAdmin()
        {
            // Arrange
            var sportId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "User") // Not an admin
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims, "TestAuth"));

            _mockHttpContext.Setup(c => c.User).Returns(user);

            // Act
            var result = await InvokeDeleteSportDelegate(sportId, _mockHttpContext.Object);

            // Assert
            Assert.IsType<ForbidHttpResult>(result);

            _mockSender.Verify(x => x.Send(It.IsAny<DeleteSportCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private async Task<IResult> InvokeGetSportsDelegate()
        {
            return await ((Func<ISender, Task<IResult>>)(async (sender) =>
            {
                var result = await sender.Send(new GetSportsQuery());
                return Results.Ok(result);
            }))(_mockSender.Object);
        }

        private async Task<IResult> InvokeGetSportByIdDelegate(Guid id)
        {
            return await ((Func<Guid, ISender, Task<IResult>>)(async (sportId, sender) =>
            {
                var result = await sender.Send(new GetSportByIdQuery(sportId));

                if (result == null)
                    return Results.NotFound();

                return Results.Ok(result);
            }))(id, _mockSender.Object);
        }

        private async Task<IResult> InvokeCreateSportDelegate(CreateSportRequest request, HttpContext httpContext)
        {
            return await ((Func<CreateSportRequest, HttpContext, ISender, Task<IResult>>)(async (req, ctx, sender) =>
            {
                var roleClaim = ctx.User.FindFirst(ClaimTypes.Role);

                if (roleClaim == null || roleClaim.Value != "Admin")
                    return Results.Forbid();

                var command = new CreateSportCommand(req.Name, req.Description, req.Icon);
                var result = await sender.Send(command);
                var response = new CreateSportResult(result.Id);
                return Results.Created($"/api/sports/{result.Id}", response);
            }))(request, httpContext, _mockSender.Object);
        }

        private async Task<IResult> InvokeUpdateSportDelegate(Guid id, UpdateSportRequest request, HttpContext httpContext)
        {
            return await ((Func<Guid, UpdateSportRequest, HttpContext, ISender, Task<IResult>>)(async (sportId, req, ctx, sender) =>
            {
                var roleClaim = ctx.User.FindFirst(ClaimTypes.Role);

                if (roleClaim == null || roleClaim.Value != "Admin")
                    return Results.Forbid();

                var command = new UpdateSportCommand(sportId, req.Name, req.Description, req.Icon);
                var result = await sender.Send(command);
                var response = new UpdateSportResult(result.IsSuccess);
                return Results.Ok(response);
            }))(id, request, httpContext, _mockSender.Object);
        }

        private async Task<IResult> InvokeDeleteSportDelegate(Guid id, HttpContext httpContext)
        {
            return await ((Func<Guid, HttpContext, ISender, Task<IResult>>)(async (sportId, ctx, sender) =>
            {
                var roleClaim = ctx.User.FindFirst(ClaimTypes.Role);

                if (roleClaim == null || roleClaim.Value != "Admin")
                    return Results.Forbid();

                var command = new DeleteSportCommand(sportId);
                var result = await sender.Send(command);
                var response = new DeleteSportResult(result.IsSuccess, result.Message);
                return Results.Ok(response);
            }))(id, httpContext, _mockSender.Object);
        }
    }
}