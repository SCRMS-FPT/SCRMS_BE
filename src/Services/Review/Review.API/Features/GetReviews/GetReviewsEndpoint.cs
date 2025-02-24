namespace Reviews.API.Features.GetReviews
{
    public class GetReviewsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/reviews", async (ISender sender, string subjectType, Guid subjectId, int page = 1, int limit = 10) =>
            {
                var query = new GetReviewsQuery(subjectType, subjectId, page, limit);
                var reviews = await sender.Send(query);
                return Results.Ok(reviews);
            })
            .WithName("GetReviews");
        }
    }
}