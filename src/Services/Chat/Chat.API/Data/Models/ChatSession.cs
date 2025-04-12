namespace Chat.API.Data.Models
{
    public class ChatSession
    {
        public Guid Id { get; set; }
        public Guid User1Id { get; set; }
        public Guid User2Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}