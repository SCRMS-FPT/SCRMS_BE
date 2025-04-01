using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Coach.API.Features.Promotion.CreateCoachPromotion
{
    public record CreateCoachPromotionRequest(
        Guid? PackageId, // Added PackageId
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

                // Check if the authenticated user is the owner of the coach profile
                if (userId != coachId)
                    return Results.Forbid();

                var command = new CreateCoachPromotionCommand(
                    coachId,
                    request.PackageId, // Include PackageId
                    request.Description,
                    request.DiscountType,
                    request.DiscountValue,
                    request.ValidFrom,
                    request.ValidTo);

                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .RequireAuthorization("Coach")
            .WithName("CreateCoachPromotion")
            .Produces<CreateCoachPromotionResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithTags("Promotion");
        }
    }
}