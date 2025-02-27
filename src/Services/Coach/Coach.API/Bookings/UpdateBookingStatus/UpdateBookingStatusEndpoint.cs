using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Coach.API.Bookings.UpdateBooking
{
    public class UpdateBookingStatusEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/bookings/{bookingId:guid}", async (
                Guid bookingId,
                [FromBody] string status,
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                var coachIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub);

                if (coachIdClaim == null || !Guid.TryParse(coachIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                await sender.Send(new UpdateBookingStatusQuery(bookingId, status));
                return Results.NoContent();
            })
            .RequireAuthorization()
            .WithName("UpdateBookingStatus")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update Booking Status")
            .WithDescription("Update the status of a booking (confirmed/cancelled).");
        }
    }
}