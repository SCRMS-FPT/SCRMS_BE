namespace Reviews.API.Data.Models
{
    public class ReviewFlag
    {
        public Guid Id { get; set; }
        public Guid ReviewId { get; set; }
        public Guid ReportedBy { get; set; }
        public string FlagReason { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}