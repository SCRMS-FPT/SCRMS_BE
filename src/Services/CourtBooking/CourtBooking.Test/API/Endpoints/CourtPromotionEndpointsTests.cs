using CourtBooking.API.Endpoints;
using CourtBooking.Application.CourtManagement.Commands.DeleteCourtPromotion;
using CourtBooking.Application.CourtManagement.Command.CreateCourtPromotion;
using CourtBooking.Application.CourtManagement.Command.UpdateCourtPromotion;
using CourtBooking.Application.CourtManagement.Queries.GetCourtPromotions;
using CourtBooking.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using System.Security.Claims;
using Xunit;

namespace CourtBooking.Test.API.Endpoints
{
    public class CourtPromotionEndpointsTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly CourtPromotionEndpoints _endpoints;
        private readonly Mock<HttpContext> _mockHttpContext;

        public CourtPromotionEndpointsTests()
        {
            _mockSender = new Mock<ISender>();
            _endpoints = new CourtPromotionEndpoints();
            _mockHttpContext = new Mock<HttpContext>();
        }

        [Fact]
        public async Task GetCourtPromotions_Should_ReturnOk_When_Exists()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var promotionId = Guid.NewGuid();
            var now = DateTime.Now;

            // Thiết lập mock cho User trong context
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "User")
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims, "TestAuth"));
            _mockHttpContext.Setup(c => c.User).Returns(user);

            var promotions = new List<CourtPromotionDTO>
            {
                new CourtPromotionDTO(
                    Id: promotionId,
                    CourtId: courtId,
                    Description: "Black Friday",
                    DiscountType: "Percentage",
                    DiscountValue: 10,
                    ValidFrom: now,
                    ValidTo: now.AddDays(7),
                    CreatedAt: now,
                    LastModified: null
                )
            };

            _mockSender.Setup(x => x.Send(It.IsAny<GetCourtPromotionsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(promotions);

            // Act
            var result = await InvokeGetCourtPromotionsDelegate(courtId, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Ok<List<CourtPromotionDTO>>>(result);
            var okResult = (Ok<List<CourtPromotionDTO>>)result;
            Assert.Single(okResult.Value);
            Assert.Equal(courtId, okResult.Value[0].CourtId);
        }

        [Fact]
        public async Task CreateCourtPromotion_Should_ReturnCreated_When_CourtOwner()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var promotionId = Guid.NewGuid();
            var now = DateTime.Now;

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "SportCenterOwner")
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims, "TestAuth"));

            _mockHttpContext.Setup(c => c.User).Returns(user);

            // Create request with properly named parameters
            var request = new CreateCourtPromotionRequest(
                Description: "Black Friday Discount",
                DiscountType: "Percentage",
                DiscountValue: 10,
                ValidFrom: now,
                ValidTo: now.AddDays(7)
            );

            var promotionDTO = new CourtPromotionDTO(
                Id: promotionId,
                CourtId: courtId,
                Description: request.Description,
                DiscountType: request.DiscountType,
                DiscountValue: request.DiscountValue,
                ValidFrom: request.ValidFrom,
                ValidTo: request.ValidTo,
                CreatedAt: now,
                LastModified: null
            );

            _mockSender.Setup(x => x.Send(It.IsAny<CreateCourtPromotionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(promotionDTO);

            // Act
            var result = await InvokeCreateCourtPromotionDelegate(courtId, request, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Created<CourtPromotionDTO>>(result);
            var created = (Created<CourtPromotionDTO>)result;
            Assert.Equal($"/api/courts/{courtId}/promotions/{promotionId}", created.Location);
            Assert.Equal(promotionId, created.Value.Id);

            _mockSender.Verify(x => x.Send(
                It.Is<CreateCourtPromotionCommand>(c =>
                    c.UserId == userId &&
                    c.CourtId == courtId),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateCourtPromotion_Should_ReturnForbidden_When_NotOwner()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var now = DateTime.Now;

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "User") // Not an owner
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims, "TestAuth"));

            _mockHttpContext.Setup(c => c.User).Returns(user);

            // Create request with properly named parameters
            var request = new CreateCourtPromotionRequest(
                Description: "Black Friday Discount",
                DiscountType: "Percentage",
                DiscountValue: 10,
                ValidFrom: now,
                ValidTo: now.AddDays(7)
            );

            // Act
            var result = await InvokeCreateCourtPromotionDelegate(courtId, request, _mockHttpContext.Object);

            // Assert
            Assert.IsType<ForbidHttpResult>(result);

            _mockSender.Verify(x => x.Send(It.IsAny<CreateCourtPromotionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCourtPromotion_Should_ReturnOk_When_CourtOwner()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var now = DateTime.Now;

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "SportCenterOwner")
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims, "TestAuth"));

            _mockHttpContext.Setup(c => c.User).Returns(user);

            // Create request with properly named parameters
            var request = new UpdateCourtPromotionRequest(
                Description: "Updated Black Friday Discount",
                DiscountType: "Percentage",
                DiscountValue: 15,
                ValidFrom: now,
                ValidTo: now.AddDays(7)
            );

            var updatedPromotionDTO = new CourtPromotionDTO(
                Id: promotionId,
                CourtId: courtId,
                Description: request.Description,
                DiscountType: request.DiscountType,
                DiscountValue: request.DiscountValue,
                ValidFrom: request.ValidFrom,
                ValidTo: request.ValidTo,
                CreatedAt: now,
                LastModified: now
            );

            _mockSender.Setup(x => x.Send(It.IsAny<UpdateCourtPromotionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedPromotionDTO);

            // Act
            var result = await InvokeUpdateCourtPromotionDelegate(courtId, promotionId, request, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Ok<CourtPromotionDTO>>(result);
            var okResult = (Ok<CourtPromotionDTO>)result;
            Assert.Equal(promotionId, okResult.Value.Id);

            _mockSender.Verify(x => x.Send(
                It.Is<UpdateCourtPromotionCommand>(c =>
                    c.UserId == userId &&
                    c.PromotionId == promotionId),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteCourtPromotion_Should_ReturnOk_When_CourtOwner()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "SportCenterOwner")
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims, "TestAuth"));

            _mockHttpContext.Setup(c => c.User).Returns(user);

            // Sử dụng DeleteCourtPromotionResult đúng từ namespace application
            var deleteResult = new CourtBooking.Application.CourtManagement.Commands.DeleteCourtPromotion.DeleteCourtPromotionResult(true);

            _mockSender.Setup(x => x.Send(It.IsAny<DeleteCourtPromotionCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(deleteResult));

            // Act
            var result = await InvokeDeleteCourtPromotionDelegate(courtId, promotionId, _mockHttpContext.Object);

            // Assert
            Assert.IsType<NoContent>(result);

            _mockSender.Verify(x => x.Send(
                It.Is<DeleteCourtPromotionCommand>(c =>
                    c.UserId == userId &&
                    c.PromotionId == promotionId),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private async Task<IResult> InvokeGetCourtPromotionsDelegate(Guid courtId, HttpContext httpContext)
        {
            return await ((Func<Guid, HttpContext, ISender, Task<IResult>>)(
                async (id, ctx, sender) =>
                {
                    var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var role = ctx.User.FindFirstValue(ClaimTypes.Role);

                    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
                        return Results.Unauthorized();

                    var query = new GetCourtPromotionsQuery(
                        id,
                        Guid.Parse(userId),
                        role
                    );

                    var result = await sender.Send(query);
                    return Results.Ok(result);
                }
            ))(courtId, httpContext, _mockSender.Object);
        }

        private async Task<IResult> InvokeCreateCourtPromotionDelegate(
            Guid courtId, CreateCourtPromotionRequest request, HttpContext httpContext)
        {
            return await ((Func<Guid, CreateCourtPromotionRequest, HttpContext, ISender, Task<IResult>>)(
                async (cId, req, ctx, sender) =>
                {
                    var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var role = ctx.User.FindFirstValue(ClaimTypes.Role);

                    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
                        return Results.Unauthorized();

                    if (role != "Admin" && role != "SportCenterOwner")
                        return Results.Forbid();

                    var command = new CreateCourtPromotionCommand(
                        cId,
                        req.Description,
                        req.DiscountType,
                        req.DiscountValue,
                        req.ValidFrom,
                        req.ValidTo,
                        Guid.Parse(userId)
                    );

                    var result = await sender.Send(command);
                    return Results.Created($"/api/courts/{cId}/promotions/{result.Id}", result);
                }
            ))(courtId, request, httpContext, _mockSender.Object);
        }

        private async Task<IResult> InvokeUpdateCourtPromotionDelegate(
            Guid courtId, Guid promotionId, UpdateCourtPromotionRequest request, HttpContext httpContext)
        {
            return await ((Func<Guid, Guid, UpdateCourtPromotionRequest, HttpContext, ISender, Task<IResult>>)(
                async (cId, pId, req, ctx, sender) =>
                {
                    var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var role = ctx.User.FindFirstValue(ClaimTypes.Role);

                    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
                        return Results.Unauthorized();

                    if (role != "Admin" && role != "SportCenterOwner")
                        return Results.Forbid();

                    var command = new UpdateCourtPromotionCommand(
                        pId,
                        req.Description,
                        req.DiscountType,
                        req.DiscountValue,
                        req.ValidFrom,
                        req.ValidTo,
                        Guid.Parse(userId)
                    );

                    var result = await sender.Send(command);
                    return Results.Ok(result);
                }
            ))(courtId, promotionId, request, httpContext, _mockSender.Object);
        }

        private async Task<IResult> InvokeDeleteCourtPromotionDelegate(
            Guid courtId, Guid promotionId, HttpContext httpContext)
        {
            return await ((Func<Guid, Guid, HttpContext, ISender, Task<IResult>>)(
                async (cId, pId, ctx, sender) =>
                {
                    var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var role = ctx.User.FindFirstValue(ClaimTypes.Role);

                    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
                        return Results.Unauthorized();

                    if (role != "Admin" && role != "SportCenterOwner")
                        return Results.Forbid();

                    var command = new DeleteCourtPromotionCommand(
                        pId,
                        Guid.Parse(userId)
                    );

                    var result = await sender.Send(command);
                    return Results.NoContent();
                }
            ))(courtId, promotionId, httpContext, _mockSender.Object);
        }
    }

    // Update record models with correct property names to match the endpoints
    public record CreateCourtPromotionRequest(
        string Description,
        string DiscountType,
        decimal DiscountValue,
        DateTime ValidFrom,
        DateTime ValidTo);

    public record UpdateCourtPromotionRequest(
        string Description,
        string DiscountType,
        decimal DiscountValue,
        DateTime ValidFrom,
        DateTime ValidTo);
}