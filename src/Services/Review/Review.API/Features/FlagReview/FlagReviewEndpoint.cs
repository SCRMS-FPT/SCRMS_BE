using System.Security.Claims;

namespace Reviews.API.Features.FlagReview
{
    public class FlagReviewEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/reviews/{reviewId}/flag", async (Guid reviewId, FlagReviewRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var command = new FlagReviewCommand(reviewId, userId, request.FlagReason);
                var flagId = await sender.Send(command);
                return Results.Created($"/api/reviews/{reviewId}/flags/{flagId}", new { Id = flagId });
            })
            .RequireAuthorization()
            .WithName("FlagReview");
        }
    }

    public record FlagReviewRequest(string FlagReason);
}