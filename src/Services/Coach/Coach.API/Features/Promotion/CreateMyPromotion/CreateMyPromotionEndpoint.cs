using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Coach.API.Features.Promotion.CreateCoachPromotion;

namespace Coach.API.Features.Promotion.CreateMyPromotion
{
    public record CreateMyPromotionRequest(
        Guid? PackageId,
        string Description,
        string DiscountType,
        decimal DiscountValue,
        DateOnly ValidFrom,
        DateOnly ValidTo
    );

    public class CreateMyPromotionEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/promotions", async (
                [FromBody] CreateMyPromotionRequest request,
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                              ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachId))
                    return Results.Unauthorized();

                var command = new CreateCoachPromotionCommand(
                    coachId,
                    request.PackageId,
                    request.Description,
                    request.DiscountType,
                    request.DiscountValue,
                    request.ValidFrom,
                    request.ValidTo);

                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .RequireAuthorization("Coach")
            .WithName("CreateMyPromotion")
            .Produces<CreateCoachPromotionResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Create My Promotion")
            .WithDescription("Create a promotion for the authenticated coach").WithTags("Promotion");
        }
    }
}