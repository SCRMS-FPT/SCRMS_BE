using Reviews.API.Features.GetReviewsByCoachId;
using Reviews.API.Features.GetSelfReviews;
using System.IdentityModel.Tokens.Jwt;

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
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                var query = new GetSelfReviewsQuery(coachUserId, page, limit);

                var reviews = await sender.Send(query);
                return Results.Ok(reviews);
            })
            .WithName("GetSelfReviewsByCoach");
        }
    }
}
