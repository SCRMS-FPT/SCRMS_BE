using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace Coach.API.Bookings.GetAllBooking
{
    public class GetCoachBookingsEndpoint : ICarterModule
    {
        public record GetCoachBookingByIdRequest(DateOnly? StartDate, DateOnly? EndDate, String? Status, int Page, int RecordPerPage);
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/bookings", async (
                [FromServices] ISender sender,
                HttpContext httpContext,
                [FromBody] GetCoachBookingByIdRequest request) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                var result = await sender.Send(new GetCoachBookingsQuery(coachUserId, request.Page, request.RecordPerPage, request.Status, request.StartDate, request.EndDate));
               
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