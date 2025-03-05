using Coach.API.Packages.PurchasePackage;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Coach.API.Promotion.GetAllPromotion
{
    public class GetAllPromotionEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/coaches/{coachId:guid}/promotions", async (
                [FromQuery] Guid coachId,
                [FromServices] ISender sender,
                HttpContext httpContext,
                [FromQuery] int Page = 1,
                [FromQuery] int RecordPerPage = 10) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var command = new GetAllPromotionQuery(
                    coachId,
                    Page,
                    RecordPerPage
                );
                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .RequireAuthorization("Admin")
            .WithName("GetAllPromotion")
            .Produces<List<PromotionRecord>>(StatusCodes.Status200OK);
        }
    }
}
