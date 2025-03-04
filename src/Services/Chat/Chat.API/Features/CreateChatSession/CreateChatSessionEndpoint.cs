namespace Chat.API.Features.CreateChatSession
{
    public class CreateChatSessionEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/chats", async (CreateChatSessionRequest request, ISender sender, HttpContext httpContext) =>
            {
                // Lấy UserId từ JWT
                var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var user1Id))
                    return Results.BadRequest("Invalid user ID in token");

                // Gửi command để tạo phiên chat
                var command = new CreateChatSessionCommand(user1Id, request.User2Id);
                var result = await sender.Send(command);
                return Results.Created($"/api/chats/{result.ChatSessionId}", result);
            })
            .RequireAuthorization()
            .WithName("CreateChatSession");
        }
    }

    // Payload chỉ chứa User2Id
    public record CreateChatSessionRequest(Guid User2Id);
}