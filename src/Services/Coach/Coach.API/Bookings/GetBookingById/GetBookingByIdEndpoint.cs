using Coach.API.Schedules.ViewAvailableSchedule;
using System.IdentityModel.Tokens.Jwt;
using static Coach.API.Bookings.GetBookingById.GetBookingByIdCommandHandler;

namespace Coach.API.Bookings.GetBookingById
{
    public class ViewCoachAvailabilityEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/booking/{bookingId:guid}", async (
                Guid bookingId,
                ISender sender,
                HttpContext httpContext) =>
            {
                var coachIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub);

                if (coachIdClaim == null || !Guid.TryParse(coachIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                var result = await sender.Send(new GetBookingByIdRequest(bookingId));
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetBookingById")
            .Produces<BookingDetailResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Get Booking Details")
            .WithDescription("Retrieve details of a specific booking by ID.");
        }
    }
}
