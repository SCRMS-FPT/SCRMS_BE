using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Coach.API.Features.Bookings.CreateBooking
{
    public record CreateBookingRequest(
        Guid CoachId,
        Guid SportId,
        DateTime StartTime,
        DateTime EndTime,
        Guid? PackageId
    );

    public class CreateBookingEndpoint : ICarterModule
    {
        private readonly ILogger<CreateBookingEndpoint> _logger;

        public CreateBookingEndpoint(ILogger<CreateBookingEndpoint> logger)
        {
            _logger = logger;
        }

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/bookings",
                async ([FromBody] CreateBookingRequest request, HttpContext httpContext, [FromServices] ISender sender) =>
                {
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

                    // Convert DateTime to TimeOnly
                    var startTime = TimeOnly.FromDateTime(request.StartTime);
                    var endTime = TimeOnly.FromDateTime(request.EndTime);

                    var command = new CreateBookingCommand(
                        userId,
                        request.CoachId,
                        request.SportId,
                        DateOnly.FromDateTime(request.StartTime),
                        startTime,
                        endTime,
                        request.PackageId
                    );

                    var result = await sender.Send(command);
                    return Results.Created($"/bookings/{result.Id}", result);
                })
                .WithName("CreateBooking")
                .Produces<CreateBookingResult>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithSummary("Create Booking")
                .WithDescription("Create a new booking with a coach").WithTags("Booking");
        }
    }
}