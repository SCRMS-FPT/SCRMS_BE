using System.Security.Claims;

namespace Reviews.API.Features.AdminDeleteReview
{
    public class AdminDeleteReviewEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/admin/reviews/{reviewId}", async (Guid reviewId, ISender sender, HttpContext httpContext) =>
            {
                var adminId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var command = new AdminDeleteReviewCommand(reviewId, adminId);
                await sender.Send(command);
                return Results.Ok(new { message = "Review deleted successfully" });
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("AdminDeleteReview");
        }
    }
}