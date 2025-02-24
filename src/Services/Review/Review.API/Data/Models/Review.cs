namespace Reviews.API.Data.Models
{
    public class Review
    {
        public Guid Id { get; set; }
        public Guid ReviewerId { get; set; }
        public string SubjectType { get; set; }
        public Guid SubjectId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}