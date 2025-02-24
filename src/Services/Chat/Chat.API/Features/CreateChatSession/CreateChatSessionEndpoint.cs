namespace Chat.API.Features.CreateChatSession
{
    public class CreateChatSessionEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/chats", async (CreateChatSessionRequest request, ISender sender) =>
            {
                var command = new CreateChatSessionCommand(request.User1Id, request.User2Id);
                var result = await sender.Send(command);
                return Results.Created($"/api/chats/{result.ChatSessionId}", result);
            })
            .RequireAuthorization()
            .WithName("CreateChatSession");
        }
    }

    public record CreateChatSessionRequest(Guid User1Id, Guid User2Id);
}