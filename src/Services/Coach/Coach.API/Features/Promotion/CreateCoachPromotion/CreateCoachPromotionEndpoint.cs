using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Coach.API.Features.Promotion.CreateCoachPromotion
{
    public record CreateCoachPromotionRequest(
        string Description,
        string DiscountType,
        decimal DiscountValue,
        DateOnly ValidFrom,
        DateOnly ValidTo
    );

    public class CreateCoachPromotionEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/coaches/{coachId:guid}/promotions", async (
                [FromQuery] Guid coachId,
                [FromBody] CreateCoachPromotionRequest request,
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var command = new CreateCoachPromotionCommand(coachId, request.Description, request.DiscountType, request.DiscountValue, request.ValidFrom, request.ValidTo);
                var result = await sender.Send(command);
                return Results.Created($"/promotions/{result.Id}", result);
            })
            .RequireAuthorization("Coach")
            .WithName("CreateCoachPromotion")
            .Produces(StatusCodes.Status201Created).WithTags("Promotion");
        }
    }
}