using Microsoft.AspNetCore.Mvc;

namespace Coach.API.Bookings.CreateBooking
{
    public record CreateBookingRequest(
        Guid UserId,
        Guid CoachId,
        Guid SportId,
        DateOnly BookingDate,
        TimeOnly StartTime,
        TimeOnly EndTime,
        Guid? PackageId
    );

    public class CreateBookingEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/bookings",
                async ([FromBody] CreateBookingRequest request, [FromServices] ISender sender) =>
                {
                    var command = request.Adapt<CreateBookingCommand>();
                    var result = await sender.Send(command);
                    return Results.Created($"/bookings/{result.Id}", result);
                })
                .WithName("CreateBooking")
                .Produces<CreateBookingResult>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithSummary("Create Booking")
                .WithDescription("Create a new booking with a coach");
        }
    }
}