using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Coach.API.Coaches.CreateCoach
{
    public record CreateCoachRequest(Guid SportId, string Bio, decimal RatePerHour);

    public record CreateCoachResponse(
    Guid Id,
    DateTime CreatedAt,
    List<Guid> SportIds);

    public class CreateCoachEndpoint : ICarterModule

    {
        public void AddRoutes(IEndpointRouteBuilder app)

        {
            app.MapPost("/coaches", async (
            CreateCoachRequest request,
            ISender sender,
            HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var command = new CreateCoachCommand(
                    UserId: userId,
                    Bio: request.Bio,
                    RatePerHour: request.RatePerHour,
                    SportIds: new List<Guid> { request.SportId }
                );

                var result = await sender.Send(command);
                return Results.Created($"/coaches/{result.Id}", result);
            })
            .RequireAuthorization()
            .WithName("CreateCoach")
            .Produces<CreateCoachResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Create Coach")
            .WithDescription("Create a new coach profile using authenticated user");
        }
    }
}