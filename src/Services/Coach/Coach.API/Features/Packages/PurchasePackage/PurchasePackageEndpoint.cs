using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Coach.API.Features.Packages.PurchasePackage
{
    public record PurchasePackageRequest(
    Guid PackageId);

    public class PurchasePackageEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/coach-packages/purchases", async (
                [FromBody] PurchasePackageRequest request,
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var command = new PurchasePackageCommand(
                    userId,
                    request.PackageId
                );
                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("PurchasePackage")
            .Produces<PurchasePackageResult>(StatusCodes.Status200OK).WithTags("Package");
        }
    }
}