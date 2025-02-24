using System.Security.Claims;

namespace Chat.API.Features.MarkMessageAsRead
{
    public class MarkMessageAsReadEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/chats/{chatSessionId}/messages/{messageId}/read", async (Guid chatSessionId, Guid messageId, ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user ID in token");

                var command = new MarkMessageAsReadCommand(chatSessionId, messageId, userId);
                await sender.Send(command);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("MarkMessageAsRead");
        }
    }
}