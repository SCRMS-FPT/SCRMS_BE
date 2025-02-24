
using Coach.API.Schedules.AddSchedule;
using System.IdentityModel.Tokens.Jwt;
using static Coach.API.Schedules.UpdateSchedule.UpdateCoachScheduleCommandHandler;

namespace Coach.API.Schedules.UpdateSchedule
{
    public record UpdateScheduleRequest(
        int DayOfWeek,
        TimeOnly StartTime,
        TimeOnly EndTime);

    public record UpdateScheduleResponse(Guid Id);

    public class UpdateCoachScheduleEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/schedules/{scheduleId:guid}", async (
            Guid scheduleId,
            UpdateScheduleRequest request,
            ISender sender,
            HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                var command = new UpdateScheduleCommand(
                  ScheduleId: scheduleId,
                  CoachId: coachUserId,
                  DayOfWeek: request.DayOfWeek,
                  StartTime: request.StartTime,
                  EndTime: request.EndTime);

                try
                {
                    var result = await sender.Send(command);
                    return Results.Ok(new UpdateScheduleResponse(result.Id));
                }
                catch (ScheduleNotFoundException)
                {
                    return Results.NotFound(new { message = "Schedule not found" });
                }
                catch (ScheduleConflictException)
                {
                    return Results.BadRequest(new { message = "Schedule conflict detected" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(title: "An error occurred", detail: ex.Message, statusCode: 500);
                }
            })
        .RequireAuthorization()
        .WithName("UpdateCoachSchedule")
        .Produces<UpdateScheduleResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Update Coach Schedule")
        .WithDescription("Update an existing coach schedule");
        }
    }

}
