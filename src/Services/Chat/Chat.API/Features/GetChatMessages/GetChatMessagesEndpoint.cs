namespace Chat.API.Features.GetChatMessages
{
    public class GetChatMessagesEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/chats/{chatSessionId}/messages", async (Guid chatSessionId, ISender sender, int page = 1, int limit = 10) =>
            {
                var query = new GetChatMessagesQuery(chatSessionId, page, limit);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetChatMessages");
        }
    }
}