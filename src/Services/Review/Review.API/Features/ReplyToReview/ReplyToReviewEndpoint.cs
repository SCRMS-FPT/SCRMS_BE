using System.Security.Claims;

namespace Reviews.API.Features.ReplyToReview
{
    public class ReplyToReviewEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/reviews/{reviewId}/reply", async (Guid reviewId, ReplyToReviewRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var command = new ReplyToReviewCommand(reviewId, userId, request.ReplyText);
                var replyId = await sender.Send(command);
                return Results.Created($"/api/reviews/{reviewId}/replies/{replyId}", new { Id = replyId });
            })
            .RequireAuthorization()
            .WithName("ReplyToReview");
        }
    }

    public record ReplyToReviewRequest(string ReplyText);
}