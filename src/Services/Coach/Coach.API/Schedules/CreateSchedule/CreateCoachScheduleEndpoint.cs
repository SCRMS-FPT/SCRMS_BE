using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Coach.API.Schedules.AddSchedule
{
    public record AddCoachScheduleRequest(
        int DayOfWeek,
        TimeOnly StartTime,
        TimeOnly EndTime);

    public record AddCoachScheduleResponse(Guid Id);

    public class AddCoachScheduleEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/schedules",
                async ([FromBody] AddCoachScheduleRequest request, [FromServices] ISender sender, HttpContext httpContext) =>
                {
                    var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub);

                    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachUserId))
                        return Results.Unauthorized();

                    var command = new CreateCoachScheduleCommand(
                        CoachUserId: coachUserId,
                        DayOfWeek: request.DayOfWeek,
                        StartTime: request.StartTime,
                        EndTime: request.EndTime);

                    var result = await sender.Send(command);

                    return Results.Created($"/schedules/{result.Id}", result);
                })
            .RequireAuthorization()
            .WithName("CreateCoachSchedule")
            .Produces<AddCoachScheduleResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Create Coach Schedule")
            .WithDescription("Create a new schedule for a coach");
        }
    }
}