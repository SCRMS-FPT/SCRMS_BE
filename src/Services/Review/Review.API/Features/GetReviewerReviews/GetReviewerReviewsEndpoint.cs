using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Reviews.API.Features.GetReviewerReviews
{
    public class GetReviewerReviewsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/reviews-submitted", async (
                ISender sender,
                HttpContext httpContext,
                int page = 1,
                int limit = 10) =>
            {
                // Extract user ID from JWT token
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var reviewerId))
                    return Results.Unauthorized();

                var query = new GetReviewerReviewsQuery(reviewerId, page, limit);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetReviewsSubmittedByUser")
            .WithTags("Reviews");
        }
    }
}