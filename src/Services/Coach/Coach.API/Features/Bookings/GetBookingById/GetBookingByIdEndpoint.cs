using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Coach.API.Data.Repositories;
namespace Coach.API.Features.Bookings.GetBookingById
{
    public class GetBookingByIdEndpoint : ICarterModule
    {
        private readonly ILogger<GetBookingByIdEndpoint> _logger;

        public GetBookingByIdEndpoint(ILogger<GetBookingByIdEndpoint> logger)
        {
            _logger = logger;
        }

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/booking/{bookingId:guid}", async (
                Guid bookingId,
                [FromServices] ISender sender,
                [FromServices] ICoachBookingRepository bookingRepository,
                HttpContext httpContext) =>
            {
                // Extract user ID from JWT claims
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                    ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                // Logging if userIdClaim is null
                if (userIdClaim == null)
                {
                    _logger.LogWarning("User ID claim is missing from the token.");
                    return Results.Unauthorized();
                }

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogWarning($"Failed to parse User ID claim: {userIdClaim.Value}");
                    return Results.Unauthorized();
                }

                // Get the booking first to check authorization
                var booking = await bookingRepository.GetCoachBookingByIdAsync(bookingId, CancellationToken.None);
                if (booking == null)
                    return Results.NotFound("Booking not found");

                // Check if the user is authorized to view this booking
                // User is authorized if they're the booking user or the coach
                bool isAuthorized = userId == booking.UserId || userId == booking.CoachId;

                // For admin role (if your system has admin roles)
                bool isAdmin = httpContext.User.IsInRole("Admin");

                if (!isAuthorized && !isAdmin)
                    return Results.Forbid();

                // User is authorized, proceed with the query
                var result = await sender.Send(new GetBookingByIdQuery(bookingId));
                return Results.Ok(result);
            })
            .WithName("GetBookingById")
            .Produces<BookingDetailResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Booking Details")
            .WithDescription("Retrieve details of a specific booking. Only the user who made the booking or the assigned coach can access this information.").WithTags("Booking");
        }
    }
}