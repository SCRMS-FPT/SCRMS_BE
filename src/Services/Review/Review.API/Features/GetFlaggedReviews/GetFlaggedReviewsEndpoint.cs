namespace Reviews.API.Features.GetFlaggedReviews
{
    public class GetFlaggedReviewsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/admin/reviews/flags", async (
                ISender sender,
                string? status = null,
                int page = 1,
                int limit = 10) =>
            {
                var query = new GetFlaggedReviewsQuery(page, limit, status);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("GetReviewFlags");
        }
    }
}