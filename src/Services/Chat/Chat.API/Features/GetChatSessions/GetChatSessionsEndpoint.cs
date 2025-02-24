using System.Security.Claims;

namespace Chat.API.Features.GetChatSessions
{
    public class GetChatSessionsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/chats", async (ISender sender, HttpContext httpContext, int page = 1, int limit = 10) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user ID in token");

                var query = new GetChatSessionsQuery(page, limit, userId);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetChatSessions");
        }
    }
}