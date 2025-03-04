using Coach.API.Dashboard.GetStat;
using Coach.API.Schedules.UpdateSchedule;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Coach.API.Dashboard.GetStats
{
    public class GetStatsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/dashboard/stats", async (
            [FromQuery] DateOnly? StartTime,
                [FromQuery] DateOnly? EndTime,
            [FromServices] ISender sender,
            HttpContext httpContext) =>
            {
                var command = new GetStatsCommand(StartTime, EndTime);

                var result = await sender.Send(command);
                return Results.Ok(result);
            })
        .RequireAuthorization("Coach")
        .WithName("GetStats")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get stats in a period")
        .WithDescription("Get stats in a period");
        }
    }
}