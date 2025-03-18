namespace BuildingBlocks.Messaging.Outbox
{
    public class OutboxMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Type { get; set; } = default!;
        public string Content { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public string? Error { get; set; }
    }
}