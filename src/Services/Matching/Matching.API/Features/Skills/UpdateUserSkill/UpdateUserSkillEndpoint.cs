using System.Security.Claims;

namespace Matching.API.Features.Skills.UpdateUserSkill
{
    public class UpdateUserSkillEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/matches/skills", async (UpdateUserSkillRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user ID in token");

                var command = new UpdateUserSkillCommand(userId, request.SportId, request.SkillLevel);
                await sender.Send(command);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("UpdateUserSkill");
        }
    }

    public record UpdateUserSkillRequest(Guid SportId, string SkillLevel);
}