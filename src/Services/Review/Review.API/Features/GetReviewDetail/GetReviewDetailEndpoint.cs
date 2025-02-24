namespace Reviews.API.Features.GetReviewDetail
{
    public class GetReviewDetailEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/reviews/{reviewId}", async (Guid reviewId, ISender sender) =>
            {
                var query = new GetReviewDetailQuery(reviewId);
                var review = await sender.Send(query);
                return Results.Ok(review);
            })
            .WithName("GetReviewDetail");
        }
    }
}