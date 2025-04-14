using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Coach.API.Features.Dashboard.GetUserDashboard
{
    public class GetUserDashboardEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/coach/user/dashboard", async (
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                // Extract user ID from JWT token
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                  ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var query = new GetUserDashboardQuery(userId);
                var result = await sender.Send(query);

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetUserCoachDashboard")
            .Produces<UserDashboardResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Get User Coach Dashboard")
            .WithDescription("Retrieves user dashboard information including total coach sessions and upcoming coach sessions.")
            .WithTags("Dashboard");
        }
    }
}