using Matching.API.Data;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Matching.API.Features.Swipe
{
    public class SwipeEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/matches/swipe", async (SwipeRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user id in token");

                var command = new SwipeCommand(request.SwipedUserId, request.Decision, userId);
                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("Swipe");
        }
    }

    public record SwipeRequest(Guid SwipedUserId, string Decision);
}