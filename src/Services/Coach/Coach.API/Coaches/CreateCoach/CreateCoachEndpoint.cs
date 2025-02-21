namespace Coach.API.Coaches.CreateCoach
{
    public record CreateCoachRequest(Guid UserId, int SportId, string Bio, decimal RatePerHour);

    public record CreateCoachResponse(Guid Id);

    public class CreateCoachEndpoint : ICarterModule

    {
        public void AddRoutes(IEndpointRouteBuilder app)

        {
            app.MapPost("/coaches",

            async (CreateCoachRequest request, ISender sender) =>

            {
                var command = request.Adapt<CreateCoachCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<CreateCoachResponse>();

                return Results.Created($"/coaches/{response.Id}", response);
            })

            .WithName("CreateCoach")

            .Produces<CreateCoachResponse>(StatusCodes.Status201Created)

            .ProducesProblem(StatusCodes.Status400BadRequest)

            .WithSummary("Create Coach")

            .WithDescription("Create a new coach profile");
        }
    }
}