using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Coach.API.Features.Schedules.GetMySchedules
{
    public class GetMySchedulesEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/coach/schedules/my", async (
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                // Extract coach ID from JWT token
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                              ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachId))
                    return Results.Unauthorized();

                // Create and process query
                var query = new GetMySchedulesQuery(coachId);
                var result = await sender.Send(query);

                return Results.Ok(result);
            })
            .RequireAuthorization("Coach")
            .WithName("GetMyCoachSchedules")
            .Produces<CoachSchedulesListResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Get Coach's Own Schedules")
            .WithDescription("Retrieves all schedules for the authenticated coach")
            .WithTags("Schedule");
        }
    }
}