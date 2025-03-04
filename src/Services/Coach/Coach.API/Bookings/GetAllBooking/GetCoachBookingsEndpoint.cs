using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Coach.API.Bookings.GetAllBooking
{
    public class GetCoachBookingsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/bookings", async (
                [FromServices] ISender sender,
                HttpContext httpContext,
                [FromQuery] DateOnly? StartDate,
                [FromQuery] DateOnly? EndDate,
                [FromQuery] String? Status,
                [FromQuery] int Page,
                [FromQuery] int RecordPerPage) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                     ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                var result = await sender.Send(new GetCoachBookingsQuery(coachUserId, Page, RecordPerPage, Status, StartDate, EndDate));

                return Results.Ok(result);
            })
            .RequireAuthorization("Coach")
            .WithName("GetCoachBookings")
            .Produces<List<BookingHistoryResult>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Get Coach Booking History")
            .WithDescription("Retrieve all past bookings associated with the authenticated coach.");
        }
    }
}