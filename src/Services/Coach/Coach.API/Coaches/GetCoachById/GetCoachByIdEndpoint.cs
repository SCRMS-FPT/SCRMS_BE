using Coach.API.Coaches.GetCoaches;

namespace Coach.API.Coaches.GetCoachById
{
    public class GetCoachByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/coaches/{id}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetCoachByIdQuery(id));
                return Results.Ok(result);
            })
            .WithName("GetCoachById")
            .Produces<CoachResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}