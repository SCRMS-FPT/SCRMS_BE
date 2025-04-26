using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Coach.API.Features.Bookings.UpdateBookingStatus
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
                var coachIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                        ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (coachIdClaim == null || !Guid.TryParse(coachIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                await sender.Send(new UpdateBookingStatusCommand(bookingId, status, coachUserId));
                return Results.NoContent();
            })
            .RequireAuthorization("Coach")
            .WithName("UpdateBookingStatus")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update Booking Status")
            .WithDescription("Update the status of a booking (completed/cancelled).").WithTags("Booking");
        }
    }
}