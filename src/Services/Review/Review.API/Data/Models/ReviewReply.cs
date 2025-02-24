namespace Reviews.API.Data.Models
{
    public class ReviewReply
    {
        public Guid Id { get; set; }
        public Guid ReviewId { get; set; }
        public Guid ResponderId { get; set; }
        public string ReplyText { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}