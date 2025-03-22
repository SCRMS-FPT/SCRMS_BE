using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Coach.API.Features.Schedules.CreateSchedule
{
    public record AddCoachScheduleRequest(
        int DayOfWeek,
        TimeOnly StartTime,
        TimeOnly EndTime);

    public class AddCoachScheduleEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/schedules",
                async ([FromBody] AddCoachScheduleRequest request, [FromServices] ISender sender, HttpContext httpContext) =>
                {
                    var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

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
            .RequireAuthorization("Coach")
            .WithName("CreateCoachSchedule")
            .Produces<CreateCoachScheduleResult>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Create Coach Schedule")
            .WithDescription("Create a new schedule for a coach").WithTags("Schedule");
        }
    }
}