using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BuildingBlocks.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Coach.API.Features.Bookings.GetUserBookings
{
    public class GetUserBookingsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/user-bookings", async (
                [FromServices] ISender sender,
                HttpContext httpContext,
                [FromQuery] DateOnly? StartDate,
                [FromQuery] DateOnly? EndDate,
                [FromQuery] string? Status,
                [FromQuery] int PageIndex,
                [FromQuery] int PageSize,
                [FromQuery] Guid? SportId,
                [FromQuery] Guid? CoachId,
                [FromQuery] Guid? PackageId) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                  ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var query = new GetUserBookingsQuery(
                    userId,
                    PageIndex,
                    PageSize,
                    Status,
                    StartDate,
                    EndDate,
                    SportId,
                    CoachId,
                    PackageId
                );
                var result = await sender.Send(query);

                return Results.Ok(result);
            })
            .WithName("GetUserBookings")
            .Produces<PaginatedResult<UserBookingHistoryResult>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Get User Booking History")
            .WithDescription("Retrieve all bookings made by the authenticated user.").WithTags("UserBooking");
        }
    }
}