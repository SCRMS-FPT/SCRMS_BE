using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Coach.API.Data.Repositories;
using System.Security.Claims;

namespace Coach.API.Features.Packages.UpdatePackage
{
    public record UpdatePackageRequest(
        string Name,
        string Description,
        decimal Price,
        int SessionCount,
        string Status
    );

    public class UpdatePackageEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/packages/{packageId:guid}", async (
                Guid packageId,
                [FromBody] UpdatePackageRequest request,
                [FromServices] ISender sender,
                [FromServices] ICoachPackageRepository packageRepository,
                HttpContext httpContext) =>
            {
                // Get coach ID from JWT
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                  ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachId))
                    return Results.Unauthorized();

                // Verify package belongs to this coach
                var package = await packageRepository.GetCoachPackageByIdAsync(packageId, CancellationToken.None);
                if (package == null)
                    return Results.NotFound("Package not found");

                if (package.CoachId != coachId)
                    return Results.Forbid();

                var command = new UpdatePackageCommand(
                    packageId,
                    coachId,
                    request.Name,
                    request.Description,
                    request.Price,
                    request.SessionCount,
                    request.Status
                );

                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .RequireAuthorization("Coach")
            .WithName("UpdatePackage")
            .Produces<UpdatePackageResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update Coach Package")
            .WithDescription("Update an existing package. Only the coach who created the package can update it.").WithTags("Package");
        }
    }
}