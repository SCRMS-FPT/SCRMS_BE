using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Coach.API.Features.Bookings.BlockCoachSchedule
{
    public record BlockCoachScheduleRequest(
        Guid SportId,
        DateTime BlockDate,
        DateTime StartTime,
        DateTime EndTime,
        string Notes
    );

    public class BlockCoachScheduleEndpoint : ICarterModule
    {
        private readonly ILogger<BlockCoachScheduleEndpoint> _logger;

        public BlockCoachScheduleEndpoint(ILogger<BlockCoachScheduleEndpoint> logger)
        {
            _logger = logger;
        }

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/coaches/block-schedule",
                async ([FromBody] BlockCoachScheduleRequest request, HttpContext httpContext, [FromServices] ISender sender) =>
                {
                    // Get coach ID from token
                    var coachIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                       ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                    if (coachIdClaim == null)
                    {
                        _logger.LogWarning("Coach ID claim is missing from the token.");
                        return Results.Unauthorized();
                    }

                    if (!Guid.TryParse(coachIdClaim.Value, out var coachId))
                    {
                        _logger.LogWarning($"Failed to parse Coach ID claim: {coachIdClaim.Value}");
                        return Results.Unauthorized();
                    }

                    // Convert DateTime to DateOnly and TimeOnly
                    var startTime = TimeOnly.FromDateTime(request.StartTime);
                    var endTime = TimeOnly.FromDateTime(request.EndTime);
                    var blockDate = DateOnly.FromDateTime(request.BlockDate);

                    var command = new BlockCoachScheduleCommand(
                        coachId,
                        request.SportId,
                        blockDate,
                        startTime,
                        endTime,
                        request.Notes
                    );

                    try
                    {
                        var result = await sender.Send(command);
                        return Results.Created($"/coaches/bookings/{result.BookingId}", result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error blocking coach schedule");
                        return Results.Problem(ex.Message, statusCode: 400);
                    }
                })
                .RequireAuthorization("CoachPolicy") // Add appropriate authorization
                .WithName("BlockCoachSchedule")
                .Produces<BlockCoachScheduleResult>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithSummary("Block Coach Schedule")
                .WithDescription("Block a time slot in a coach's schedule")
                .WithTags("Coach");
        }
    }
}