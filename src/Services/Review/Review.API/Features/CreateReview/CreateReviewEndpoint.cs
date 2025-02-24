using System.Security.Claims;

namespace Reviews.API.Features.CreateReview
{
    public class CreateReviewEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/reviews", async (CreateReviewRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var command = new CreateReviewCommand(userId, request.SubjectType, request.SubjectId, request.Rating, request.Comment);
                var reviewId = await sender.Send(command);
                return Results.Created($"/api/reviews/{reviewId}", new { Id = reviewId });
            })
            .RequireAuthorization()
            .WithName("CreateReview");
        }
    }

    public record CreateReviewRequest(string SubjectType, Guid SubjectId, int Rating, string? Comment);
}