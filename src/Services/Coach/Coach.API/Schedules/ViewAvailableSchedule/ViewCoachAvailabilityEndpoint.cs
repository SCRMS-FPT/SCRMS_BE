using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace Coach.API.Schedules.ViewAvailableSchedule
{
    public class ViewCoachAvailabilityEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/availability", async (
                [FromServices] ISender sender,
                HttpContext httpContext,
                int Page = 1,
                int RecordPerPage = 10) =>
            {
                var coachIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub);

                if (coachIdClaim == null || !Guid.TryParse(coachIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                var command = new ViewCoachAvailabilityCommand(coachUserId, Page, RecordPerPage);
                var availableSlots = await sender.Send(command);

                return Results.Ok(availableSlots);
            })
            .RequireAuthorization()
            .WithName("ViewCoachAvailability")
            .Produces<List<AvailableScheduleSlot>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("View Coach Availability")
            .WithDescription("Get the available schedule slots for the authenticated coach.");
        }
    }
}