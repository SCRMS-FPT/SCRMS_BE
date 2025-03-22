using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Coach.API.Features.Packages.CreatePackage
{
    public record CreatePackageRequest(
    Guid CoachId,
    string Name,
    string Description,
    decimal Price,
    int SessionCount);

    public class CreatePackageEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/packages", async (CreatePackageRequest request, HttpContext httpContext, ISender sender) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                        ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                var command = new CreatePackageCommand(
                    coachUserId,
                    request.Name,
                    request.Description,
                    request.Price,
                    request.SessionCount
                );
                var result = await sender.Send(command);
                return Results.Created($"/packages/{result.Id}", result);
            })
            .RequireAuthorization("Coach")
            .WithName("CreatePackage")
            .Produces(StatusCodes.Status201Created).WithTags("Package");
        }

        public static async Task<IResult> HandleCreatePackage(
            CreatePackageRequest request,
            ISender sender,
            HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachUserId))
                return Results.Unauthorized();

            var command = new CreatePackageCommand(
                coachUserId,
                request.Name,
                request.Description,
                request.Price,
                request.SessionCount
            );
            var result = await sender.Send(command);
            return Results.Created($"/packages/{result.Id}", result);
        }
    }
}