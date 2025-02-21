using Microsoft.IdentityModel.JsonWebTokens;

namespace Coach.API.Schedules.AddSchedule
{
    public record AddCoachScheduleRequest(
    int DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime);

    public record AddCoachScheduleResponse(Guid Id);

    public class AddCoachScheduleEndpoint : ICarterModule

    {
        public void AddRoutes(IEndpointRouteBuilder app)

        {
            app.MapPost("/schedules", async (
                AddCoachScheduleRequest request,
                ISender sender,
                HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                var command = new AddCoachScheduleCommand(
                    CoachUserId: coachUserId,
                    DayOfWeek: request.DayOfWeek,
                    StartTime: request.StartTime,
                    EndTime: request.EndTime);

                var result = await sender.Send(command);

                return Results.Created($"/schedules/{result.Id}", result);
            })
            .RequireAuthorization()
            .WithName("AddCoachSchedule")
            .Produces<AddCoachScheduleResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Add Coach Schedule")
            .WithDescription("Add a new schedule for a coach");
        }
    }
}