using Chat.API.Features.GetChatSessions;

namespace Chat.API.Data.Repositories
{
    public interface IChatSessionRepository
    {
        Task<ChatSession> GetChatSessionByIdAsync(Guid id);

        Task<ChatSession> GetChatSessionByUsersAsync(Guid user1Id, Guid user2Id);

        Task AddChatSessionAsync(ChatSession chatSession);

        Task<List<ChatSessionResponse>> GetChatSessionByUserIdAsync(Guid userId);
    }
}