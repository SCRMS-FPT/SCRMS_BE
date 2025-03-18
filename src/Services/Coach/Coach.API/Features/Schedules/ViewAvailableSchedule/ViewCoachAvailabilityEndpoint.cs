using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Coach.API.Features.Schedules.ViewAvailableSchedule
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
                var coachIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (coachIdClaim == null || !Guid.TryParse(coachIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                var command = new ViewCoachAvailabilityQuery(coachUserId, Page, RecordPerPage);
                var availableSlots = await sender.Send(command);

                return Results.Ok(availableSlots);
            })
            .RequireAuthorization("Coach")
            .WithName("ViewCoachAvailability")
            .Produces<List<AvailableScheduleSlot>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("View Coach Availability")
            .WithDescription("Get the available schedule slots for the authenticated coach.");
        }
    }
}