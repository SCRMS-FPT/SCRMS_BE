using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BuildingBlocks.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Coach.API.Features.Bookings.GetAllBooking
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
                [FromQuery] string? Status,
                [FromQuery] int PageIndex,
                [FromQuery] int PageSize,
                [FromQuery] Guid? SportId,
                [FromQuery] Guid? PackageId) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                     ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                var query = new GetCoachBookingsQuery(
                    coachUserId,
                    PageIndex,
                    PageSize,
                    Status,
                    StartDate,
                    EndDate,
                    SportId,
                    PackageId
                );
                var result = await sender.Send(query);

                return Results.Ok(result);
            })
            .RequireAuthorization("Coach")
            .WithName("GetCoachBookings")
            .Produces<PaginatedResult<BookingHistoryResult>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Get Coach Booking History")
            .WithDescription("Retrieve all past bookings associated with the authenticated coach.").WithTags("Booking");
        }
    }
}