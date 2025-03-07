using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Chat.API.Features.SendMessage
{
    public class SendMessageEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/chats/{chatSessionId}/messages", async (Guid chatSessionId, [FromBody] SendMessageRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var senderId))
                    return Results.BadRequest("Invalid sender ID in token");

                var command = new SendMessageCommand(chatSessionId, senderId, request.MessageText);
                var result = await sender.Send(command);
                return Results.Created($"/api/chats/{chatSessionId}/messages/{result.Id}", result);
            })
            .RequireAuthorization()
            .WithName("SendMessage");
        }
    }

    public record SendMessageRequest(string MessageText);
}