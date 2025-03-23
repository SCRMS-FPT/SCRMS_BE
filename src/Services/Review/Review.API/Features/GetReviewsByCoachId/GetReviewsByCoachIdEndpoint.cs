using Reviews.API.Features.GetReviews;

namespace Reviews.API.Features.GetReviewsByCoachId
{
    public class GetReviewsByCoachIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/reviews/{coachId:guid}", async (
                Guid coachId,
                ISender sender,
                int page = 1,
                int limit = 10) =>
            {
                var query = new GetReviewsByCoachIdQuery(coachId, page, limit);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetReviewsByCoachId");
        }
    }
}