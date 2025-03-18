namespace Chat.API.Data.Repositories
{
    public interface IChatMessageRepository
    {
        Task<ChatMessage> GetChatMessageByIdAsync(Guid id);

        Task<ChatMessage> GetChatMessageByIdAndSessionAsync(Guid messageId, Guid chatSessionId, Guid userId);

        Task<List<ChatMessage>> GetChatMessageByChatSessionIdAsync(Guid chatSessionId, int page, int limit);

        Task AddChatMessageAsync(ChatMessage chatMessage);

        Task UpdateChatMessageAsync(ChatMessage chatMessage);
    }
}