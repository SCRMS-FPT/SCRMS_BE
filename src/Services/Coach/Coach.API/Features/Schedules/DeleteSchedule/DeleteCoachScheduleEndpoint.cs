using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Coach.API.Features.Schedules.DeleteSchedule
{
    public class DeleteScheduleEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/schedules/{scheduleId:guid}", async (
                Guid scheduleId,
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                var command = new DeleteScheduleCommand(scheduleId, coachUserId);
                var result = await sender.Send(command);

                return result.IsDeleted ? Results.NoContent() : Results.Problem("Failed to delete schedule.");
            })
            .RequireAuthorization("Coach")
            .WithName("DeleteCoachSchedule")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Delete a Coach's Schedule")
            .WithDescription("Deletes a schedule for a coach if no bookings are associated.").WithTags("Schedule");
        }
    }
}