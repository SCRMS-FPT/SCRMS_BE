using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Coach.API.Features.Promotion.GetAllPromotion;
using MediatR;
using Carter;

namespace Coach.API.Features.Promotion.GetMyPromotions
{
    public class GetMyPromotionsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/promotions", async (
                [FromServices] ISender sender,
                HttpContext httpContext,
                [FromQuery] int Page = 1,
                [FromQuery] int RecordPerPage = 10) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                              ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachId))
                    return Results.Unauthorized();

                // Use the try-catch block to allow exceptions to propagate properly to tests
                var query = new GetAllPromotionQuery(
                    coachId,
                    Page,
                    RecordPerPage
                );
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization("Coach")
            .WithName("GetMyPromotions")
            .Produces<List<PromotionRecord>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Get My Promotions")
            .WithDescription("Get all promotions for the authenticated coach").WithTags("Promotion");
        }
    }
}