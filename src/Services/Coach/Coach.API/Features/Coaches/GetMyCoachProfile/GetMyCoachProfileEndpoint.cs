using Coach.API.Data.Models;
using Coach.API.Features.Coaches.GetCoaches;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Coach.API.Features.Coaches.GetMyCoachProfile
{
    public class GetMyCoachProfileEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/coaches/me", async (HttpContext httpContext, [FromServices] ISender sender) =>
            {
                // Extract user ID from JWT token
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                                 httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Results.Unauthorized();
                }

                var query = new GetMyCoachProfileQuery(userId);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetMyCoachProfile")
            .RequireAuthorization("Coach") // Restrict to Coach role
            .Produces<CoachResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Coach")
            .WithSummary("Get authenticated coach's profile")
            .WithDescription("Retrieves the profile information for the currently authenticated coach based on their JWT token");
        }
    }
}