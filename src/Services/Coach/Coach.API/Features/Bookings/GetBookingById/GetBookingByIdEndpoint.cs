using System.IdentityModel.Tokens.Jwt;
using static Coach.API.Features.Bookings.GetBookingById.GetBookingByIdQueryHandler;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Coach.API.Features.Bookings.GetBookingById
{
    public class ViewCoachAvailabilityEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/booking/{bookingId:guid}", async (
                Guid bookingId,
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                var coachIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                        ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (coachIdClaim == null || !Guid.TryParse(coachIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                var result = await sender.Send(new GetBookingByIdQuery(bookingId));
                return Results.Ok(result);
            })
            .RequireAuthorization("Coach")
            .WithName("GetBookingById")
            .Produces<BookingDetailResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Get Booking Details")
            .WithDescription("Retrieve details of a specific booking by ID.");
        }
    }
}