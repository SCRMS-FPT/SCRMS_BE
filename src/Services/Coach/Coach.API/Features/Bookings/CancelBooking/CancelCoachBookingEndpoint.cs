using BuildingBlocks.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Coach.API.Features.Bookings.CancelBooking
{
    public class CancelCoachBookingEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/bookings/{bookingId:guid}/cancel", async (
                Guid bookingId,
                [FromBody] CancelCoachBookingRequest request,
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                      ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value ?? "";

                try
                {
                    var result = await sender.Send(new CancelCoachBookingCommand(
                        bookingId,
                        request.CancellationReason,
                        DateTime.UtcNow,
                        userId,
                        userRole
                    ));

                    return Results.Ok(result);
                }
                catch (NotFoundException ex)
                {
                    return Results.NotFound(new { message = ex.Message });
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Results.Forbid();
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            })
            .RequireAuthorization()
            .WithName("CancelCoachBooking")
            .Produces<CancelCoachBookingResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Cancel Coach Booking")
            .WithDescription("Cancel a coach booking and process refund if eligible.").WithTags("Booking");
        }
    }
}
