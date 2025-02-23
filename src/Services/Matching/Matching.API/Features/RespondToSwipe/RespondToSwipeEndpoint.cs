using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Matching.API.Features.RespondToSwipe
{
    public class RespondToSwipeEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/matches/respond", async (RespondRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user id in token");

                var command = new RespondToSwipeCommand(request.SwipeActionId, request.Decision, userId);
                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("RespondToSwipe");
        }
    }

    public record RespondRequest(Guid SwipeActionId, string Decision);
}