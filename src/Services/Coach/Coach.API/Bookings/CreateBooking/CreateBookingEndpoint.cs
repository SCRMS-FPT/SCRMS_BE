namespace Coach.API.Bookings.CreateBooking
{
    public record CreateBookingRequest(

Guid UserId,

Guid CoachId,

Guid SportId,

DateTime StartDateTime,

DateTime EndDateTime);

    public record CreateBookingResponse(Guid Id);

    public class CreateBookingEndpoint : ICarterModule

    {
        public void AddRoutes(IEndpointRouteBuilder app)

        {
            app.MapPost("/bookings",

            async (CreateBookingRequest request, ISender sender) =>

            {
                var command = request.Adapt<CreateBookingCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<CreateBookingResponse>();

                return Results.Created($"/bookings/{response.Id}", response);
            })

            .WithName("CreateBooking")

            .Produces<CreateBookingResponse>(StatusCodes.Status201Created)

            .ProducesProblem(StatusCodes.Status400BadRequest)

            .WithSummary("Create Booking")

            .WithDescription("Create a new booking with a coach");
        }
    }
}