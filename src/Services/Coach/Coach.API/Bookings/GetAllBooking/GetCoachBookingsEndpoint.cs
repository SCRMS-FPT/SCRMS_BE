using System.IdentityModel.Tokens.Jwt;

namespace Coach.API.Bookings.GetAllBooking
{
    public class GetCoachBookingsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/bookings", async (
                ISender sender,
                HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                var result = await sender.Send(new GetCoachBookingsQuery(coachUserId));
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetCoachBookings")
            .Produces<List<BookingHistoryResult>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Get Coach Booking History")
            .WithDescription("Retrieve all past bookings associated with the authenticated coach.");
        }
    }
}
