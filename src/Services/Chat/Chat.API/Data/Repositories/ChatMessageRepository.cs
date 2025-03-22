using Microsoft.EntityFrameworkCore;

namespace Chat.API.Data.Repositories
{
    public class ChatMessageRepository : IChatMessageRepository
    {
        private readonly ChatDbContext _context;

        public ChatMessageRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<ChatMessage> GetChatMessageByIdAsync(Guid id)
        {
            return await _context.ChatMessages.FindAsync(id);
        }

        public async Task<ChatMessage> GetChatMessageByIdAndSessionAsync(Guid messageId, Guid chatSessionId, Guid userId)
        {
            return await _context.ChatMessages
                .FirstOrDefaultAsync(cm => cm.Id == messageId && cm.ChatSessionId == chatSessionId && cm.SenderId == userId);
        }

        public async Task<List<ChatMessage>> GetChatMessageByChatSessionIdAsync(Guid chatSessionId, int page, int limit)
        {
            return await _context.ChatMessages
                .Where(cm => cm.ChatSessionId == chatSessionId)
                .OrderByDescending(cm => cm.SentAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();
        }

        public async Task AddChatMessageAsync(ChatMessage chatMessage)
        {
            await _context.ChatMessages.AddAsync(chatMessage);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateChatMessageAsync(ChatMessage chatMessage)
        {
            _context.ChatMessages.Update(chatMessage);
            await _context.SaveChangesAsync();
        }
    }
}