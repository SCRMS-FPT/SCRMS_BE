using System.Security.Claims;

namespace Reviews.API.Features.UpdateReview
{
    public class UpdateReviewEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/reviews/{reviewId}", async (Guid reviewId, UpdateReviewRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var command = new UpdateReviewCommand(reviewId, userId, request.Rating, request.Comment);
                await sender.Send(command);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("UpdateReview");
        }
    }

    public record UpdateReviewRequest(int Rating, string? Comment);
}