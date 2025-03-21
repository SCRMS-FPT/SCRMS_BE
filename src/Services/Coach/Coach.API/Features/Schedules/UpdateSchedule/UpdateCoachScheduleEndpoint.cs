using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Coach.API.Features.Schedules.UpdateSchedule
{
    public record UpdateScheduleRequest(
        int DayOfWeek,
        TimeOnly StartTime,
        TimeOnly EndTime);

    public class UpdateCoachScheduleEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/schedules/{scheduleId:guid}", async (
            Guid scheduleId,
           [FromBody] UpdateScheduleRequest request,
            [FromServices] ISender sender,
            HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                var command = new UpdateScheduleCommand(
                  ScheduleId: scheduleId,
                  CoachId: coachUserId,
                  DayOfWeek: request.DayOfWeek,
                  StartTime: request.StartTime,
                  EndTime: request.EndTime);

                //try
                //{
                var result = await sender.Send(command);
                return result.IsUpdated ? Results.NoContent() : Results.Problem("Failed to update schedule.");
                //}
                //catch (ScheduleNotFoundException)
                //{
                //    return Results.NotFound(new { message = "Schedule not found" });
                //}
                //catch (ScheduleConflictException)
                //{
                //    return Results.BadRequest(new { message = "Schedule conflict detected" });
                //}
                //catch (Exception ex)
                //{
                //    return Results.Problem(title: "An error occurred", detail: ex.Message, statusCode: 500);
                //}
            })
        .RequireAuthorization("Coach")
        .WithName("UpdateCoachSchedule")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Update Coach Schedule")
        .WithDescription("Update an existing coach schedule").WithTags("Schedule");
        }
    }
}