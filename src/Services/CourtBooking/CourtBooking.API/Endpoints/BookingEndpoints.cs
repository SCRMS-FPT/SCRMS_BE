using CourtBooking.Application.BookingManagement.Command.CreateBooking;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.API.Endpoints
{
    public record CreateBookingRequest(BookingCreateDTO Booking);
    public record CreateBookingResponse(Guid Id);

    public class BookingEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/bookings").WithTags("Booking");

            // Create Booking
            group.MapPost("/", async ([FromBody] CreateBookingRequest request, ISender sender) =>
            {
                var command = new CreateBookingCommand(request.Booking);
                var result = await sender.Send(command);
                var response = new CreateBookingResponse(result.Id);
                return Results.Created($"/api/bookings/{response.Id}", response);
            })
            .WithName("CreateBooking")
            .Produces<CreateBookingResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Booking")
            .WithDescription("Create a new booking");
        }
    }
}