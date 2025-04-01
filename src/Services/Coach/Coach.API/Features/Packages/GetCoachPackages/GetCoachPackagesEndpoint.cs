using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Coach.API.Features.Packages.GetActivePackages;

namespace Coach.API.Features.Packages.GetCoachPackages
{
    public class GetCoachPackagesEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/coach-packages", async (
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                // Get coach ID from JWT token
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                  ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachId))
                    return Results.Unauthorized();

                var query = new GetCoachPackagesQuery(coachId);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization("Coach")
            .WithName("GetCoachPackages")
            .Produces<List<PackageResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Get Coach Packages")
            .WithDescription("Retrieve all packages created by the authenticated coach.").WithTags("Package");
        }
    }
}