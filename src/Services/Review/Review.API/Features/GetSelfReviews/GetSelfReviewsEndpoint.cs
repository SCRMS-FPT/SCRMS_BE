using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Reviews.API.Features.GetSelfReviews
{
    public class GetSelfReviewsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/reviews-coach", async (
                 ISender sender,
                 HttpContext httpContext,
                 int page = 1, int limit = 10) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                var query = new GetSelfReviewsQuery(coachUserId, page, limit);
                var result = await sender.Send(query); // result là PaginatedResult<ReviewResponse>
                return Results.Ok(result);
            })
             .WithName("GetSelfReviewsByCoach");
        }
    }
}