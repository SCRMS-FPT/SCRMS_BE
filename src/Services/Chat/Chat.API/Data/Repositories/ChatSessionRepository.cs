using Chat.API.Features.GetChatSessions;
using Microsoft.EntityFrameworkCore;

namespace Chat.API.Data.Repositories
{
    public class ChatSessionRepository : IChatSessionRepository
    {
        private readonly ChatDbContext _context;

        public ChatSessionRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<ChatSession> GetChatSessionByIdAsync(Guid id)
        {
            return await _context.ChatSessions.FindAsync(id);
        }

        public async Task<ChatSession> GetChatSessionByUsersAsync(Guid user1Id, Guid user2Id)
        {
            return await _context.ChatSessions
                .FirstOrDefaultAsync(cs => (cs.User1Id == user1Id && cs.User2Id == user2Id) ||
                                           (cs.User1Id == user2Id && cs.User2Id == user1Id));
        }

        public async Task AddChatSessionAsync(ChatSession chatSession)
        {
            await _context.ChatSessions.AddAsync(chatSession);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ChatSessionResponse>> GetChatSessionByUserIdAsync(Guid userId)
        {
            var chatSessions = await _context.ChatSessions
                .Where(cs => cs.User1Id == userId || cs.User2Id == userId)
                .ToListAsync();

            // Chuyển đổi từ ChatSession sang ChatSessionResponse
            return chatSessions.Select(cs => new ChatSessionResponse(
                Id: cs.Id,
                PartnerId: cs.User1Id == userId ? cs.User2Id : cs.User1Id,
                UpdatedAt: cs.UpdatedAt
            )).ToList();
        }
    }
}