using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Coach.API.Features.Promotion.DeleteCoachPromotion
{
    public class DeleteCoachPromotionEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/coaches/promotions/{promotionId:guid}", async (
                Guid promotionId,
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var command = new DeleteCoachPromotionCommand(promotionId);
                var result = await sender.Send(command);
                return Results.Ok();
            })
            .RequireAuthorization("Coach")
            .WithName("DeletePromotion")
            .Produces(StatusCodes.Status200OK).WithTags("Promotion");
        }
    }
}