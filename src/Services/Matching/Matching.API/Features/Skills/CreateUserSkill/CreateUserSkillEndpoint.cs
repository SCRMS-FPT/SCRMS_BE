using System.Security.Claims;

namespace Matching.API.Features.Skills.CreateUserSkill
{
    public class CreateUserSkillEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/matches/skills", async (CreateUserSkillRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user ID in token");

                var command = new CreateUserSkillCommand(userId, request.SportId, request.SkillLevel);
                await sender.Send(command);
                return Results.Created($"/api/matches/skills/{request.SportId}", null);
            })
            .RequireAuthorization()
            .WithName("CreateUserSkill")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithDescription("Create a new skill record for the authenticated user");
        }
    }

    public record CreateUserSkillRequest(Guid SportId, string SkillLevel);
}