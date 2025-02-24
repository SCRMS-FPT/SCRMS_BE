namespace Chat.API.Data.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public Guid ChatSessionId { get; set; }
        public Guid SenderId { get; set; }
        public string MessageText { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}