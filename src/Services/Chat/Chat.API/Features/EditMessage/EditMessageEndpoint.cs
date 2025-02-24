using System.Security.Claims;

namespace Chat.API.Features.EditMessage
{
    public class EditMessageEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/chats/{chatSessionId}/messages/{messageId}", async (Guid chatSessionId, Guid messageId, EditMessageRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user ID in token");

                var command = new EditMessageCommand(chatSessionId, messageId, request.MessageText, userId);
                await sender.Send(command);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("EditMessage");
        }
    }

    public record EditMessageRequest(string MessageText);
}