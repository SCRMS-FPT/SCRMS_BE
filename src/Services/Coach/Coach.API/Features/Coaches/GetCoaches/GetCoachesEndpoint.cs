using Microsoft.AspNetCore.Mvc;

namespace Coach.API.Features.Coaches.GetCoaches
{
    public class GetCoachesEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/coaches", async (
                [FromQuery] string? name,
                [FromQuery] Guid? sportId,
                [FromQuery] decimal? minPrice,
                [FromQuery] decimal? maxPrice,
                [FromServices] ISender sender) =>
            {
                var query = new GetCoachesQuery(name, sportId, minPrice, maxPrice);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetCoaches")
            .Produces<IEnumerable<CoachResponse>>()
            .WithTags("Coach")
            .WithDescription("Get all coaches with optional filtering by name, sport, and price range");
        }
    }
}