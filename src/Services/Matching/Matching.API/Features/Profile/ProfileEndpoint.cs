using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Matching.API.Features.Profile
{
    public class ProfileEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/matches/profile", async (ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user id in token");

                var query = new GetProfileQuery(userId);
                var profile = await sender.Send(query);
                return Results.Ok(profile);
            })
            .RequireAuthorization()
            .WithName("GetProfile");

            app.MapPut("/api/matches/profile", async (UpdateProfileRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user id in token");

                var command = new UpdateProfileCommand(userId, request.SelfIntroduction);
                await sender.Send(command);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("UpdateProfile");
        }
    }

    public record UpdateProfileRequest(string SelfIntroduction);
}