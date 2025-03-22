namespace Reviews.API.Features.GetReviewReplies
{
    public class GetReviewRepliesEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/reviews/{reviewId}/replies", async (
                ISender sender,
                Guid reviewId,
                int page = 1,
                int limit = 10) =>
            {
                var query = new GetReviewRepliesQuery(reviewId, page, limit);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetReviewReplies");
        }
    }
}