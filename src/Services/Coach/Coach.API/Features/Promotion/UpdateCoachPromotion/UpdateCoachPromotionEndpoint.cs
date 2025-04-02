using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Coach.API.Data;

namespace Coach.API.Features.Promotion.UpdateCoachPromotion
{
    public record UpdateCoachPromotionRequest(
        Guid? PackageId, // Added PackageId
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
                Guid promotionId,
                [FromBody] UpdateCoachPromotionRequest request,
                [FromServices] ISender sender,
                [FromServices] CoachDbContext context,
                HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                // Verify the promotion belongs to the authenticated coach
                var promotion = await context.CoachPromotions.FirstOrDefaultAsync(p => p.Id == promotionId);
                if (promotion == null)
                    return Results.NotFound("Promotion not found");

                if (promotion.CoachId != userId)
                    return Results.Forbid();

                var command = new UpdateCoachPromotionCommand(
                    promotionId,
                    request.PackageId, // Include PackageId
                    request.Description,
                    request.DiscountType,
                    request.DiscountValue,
                    request.ValidFrom,
                    request.ValidTo);

                var result = await sender.Send(command);
                return Results.Ok();
            })
            .RequireAuthorization("Coach")
            .WithName("UpdatePromotion")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Promotion");
        }
    }
}