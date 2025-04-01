using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Coach.API.Data.Repositories;

namespace Coach.API.Features.Packages.DeletePackage
{
    public class DeletePackageEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/packages/{packageId:guid}", async (
                Guid packageId,
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

                var command = new DeletePackageCommand(packageId, coachId);
                var result = await sender.Send(command);

                return Results.Ok(result);
            })
            .RequireAuthorization("Coach")
            .WithName("DeletePackage")
            .Produces<DeletePackageResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete/Deactivate Coach Package")
            .WithDescription("Deactivate an existing package. Only the coach who created the package can deactivate it.").WithTags("Package");
        }
    }
}