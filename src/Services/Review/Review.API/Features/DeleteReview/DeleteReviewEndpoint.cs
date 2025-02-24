using System.Security.Claims;

namespace Reviews.API.Features.DeleteReview
{
    public class DeleteReviewEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/reviews/{reviewId}", async (Guid reviewId, ISender sender, HttpContext httpContext) =>
            {
                var userId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var command = new DeleteReviewCommand(reviewId, userId);
                await sender.Send(command);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("DeleteReview");
        }
    }
}