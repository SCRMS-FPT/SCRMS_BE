namespace Coach.API.Coaches.GetCoaches
{
    public class GetCoachesEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/coaches", async (ISender sender) =>
            {
                var result = await sender.Send(new GetCoachesQuery());
                return Results.Ok(result);
            })
            .WithName("GetCoaches")
            .Produces<IEnumerable<CoachResponse>>();
        }
    }
}