using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Coach.API.Features.Promotion.UpdateCoachPromotion
{
    public record UpdateCoachPromotionRequest(
        string Description,
        string DiscountType,
        decimal DiscountValue,
        DateOnly ValidFrom,
        DateOnly ValidTo
    );
    public class UpdateCoachPromotionEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/coaches/promotions/{promotionId:guid}", async (
                [FromQuery] Guid promotionId,
                [FromBody] UpdateCoachPromotionRequest request,
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var command = new UpdateCoachPromotionCommand(promotionId, request.Description, request.DiscountType, request.DiscountValue, request.ValidFrom, request.ValidTo);
                var result = await sender.Send(command);
                return Results.Ok();
            })
            .RequireAuthorization("Admin")
            .WithName("UpdateCoachPromotion")
            .Produces(StatusCodes.Status200OK);
        }
    }
}
