using CourtBooking.Application.CourtManagement.Command.CreateCourtPromotion;
using CourtBooking.Application.CourtManagement.Command.UpdateCourtPromotion;
using CourtBooking.Application.CourtManagement.Commands.DeleteCourtPromotion;
using CourtBooking.Application.CourtManagement.Queries.GetCourtPromotions;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CourtBooking.API.Endpoints
{
    public class CourtPromotionEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/courts/promotions").WithTags("Court Promotion");

            group.MapGet("/{courtId:guid}/promotions", async (
                Guid courtId,
                HttpContext httpContext,
                [FromServices] ISender sender) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);

                if (userIdClaim == null || roleClaim == null)
                    return Results.Unauthorized();

                var userId = Guid.Parse(userIdClaim.Value);
                var role = roleClaim.Value;

                var query = new GetCourtPromotionsQuery(courtId, userId, role);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetCourtPromotions")
            .Produces<List<CourtPromotionDTO>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Lấy danh sách khuyến mãi của sân")
            .WithDescription("Trả về danh sách các khuyến mãi áp dụng cho sân dựa trên courtId.");

            group.MapPost("/{courtId:guid}/promotions", async (
                Guid courtId,
                [FromBody] CreateCourtPromotionRequest request,
                HttpContext httpContext,
                [FromServices] ISender sender) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);

                if (userIdClaim == null || roleClaim == null)
                    return Results.Unauthorized();

                var userId = Guid.Parse(userIdClaim.Value);
                var role = roleClaim.Value;

                if (role != "CourtOwner")
                    return Results.Forbid();

                var command = new CreateCourtPromotionCommand(courtId, request.Description, request.DiscountType, request.DiscountValue, request.ValidFrom, request.ValidTo, userId);
                var result = await sender.Send(command);
                return Results.Created($"/api/courts/{courtId}/promotions/{result.Id}", result);
            })
            .RequireAuthorization()
            .WithName("CreateCourtPromotion")
            .Produces<CourtPromotionDTO>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithSummary("Tạo khuyến mãi mới cho sân")
            .WithDescription("Tạo một khuyến mãi mới cho sân dựa trên courtId.");

            group.MapPut("/{promotionId:guid}", async (
                Guid promotionId,
                [FromBody] UpdateCourtPromotionRequest request,
                HttpContext httpContext,
                [FromServices] ISender sender) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);

                if (userIdClaim == null || roleClaim == null)
                    return Results.Unauthorized();

                var userId = Guid.Parse(userIdClaim.Value);
                var role = roleClaim.Value;

                if (role != "CourtOwner")
                    return Results.Forbid();

                var command = new UpdateCourtPromotionCommand(promotionId, request.Description, request.DiscountType, request.DiscountValue, request.ValidFrom, request.ValidTo, userId);
                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("UpdateCourtPromotion")
            .Produces<CourtPromotionDTO>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithSummary("Cập nhật khuyến mãi")
            .WithDescription("Cập nhật thông tin khuyến mãi dựa trên promotionId.");

            group.MapDelete("/{promotionId:guid}", async (
                Guid promotionId,
                HttpContext httpContext,
                [FromServices] ISender sender) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);

                if (userIdClaim == null || roleClaim == null)
                    return Results.Unauthorized();

                var userId = Guid.Parse(userIdClaim.Value);
                var role = roleClaim.Value;

                if (role != "CourtOwner")
                    return Results.Forbid();

                var command = new DeleteCourtPromotionCommand(promotionId, userId);
                await sender.Send(command);
                return Results.Ok(new { Message = "Xóa khuyến mãi thành công." });
            })
            .RequireAuthorization()
            .WithName("DeleteCourtPromotion")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithSummary("Xóa khuyến mãi")
            .WithDescription("Xóa khuyến mãi dựa trên promotionId.");
        }
    }

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