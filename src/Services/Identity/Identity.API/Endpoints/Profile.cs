using Identity.Application.Identity.Commands.UpdateProfile;
using Identity.Application.Identity.Queries.GetProfile;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Identity.API.Endpoints
{
    public class Profile : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/identity").RequireAuthorization();

            group.MapGet("/get-profile", async (ISender sender, HttpContext httpContext) =>
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
            });

            group.MapPut("/update-profile", async (UpdateProfileRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                  ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user id in token");

                var command = new UpdateProfileCommand(
                    userId,
                    request.FirstName,
                    request.LastName,
                    request.Phone,
                    request.BirthDate,
                    request.Gender
                );

                var updatedProfile = await sender.Send(command);
                return Results.Ok(updatedProfile);
            });
        }
    }

    public record UpdateProfileRequest(
         string FirstName,
         string LastName,
         string Phone,
         DateTime BirthDate,
         string Gender
    );
}