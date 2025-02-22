using System.ComponentModel.DataAnnotations;

namespace Coach.API.Models
{
    public class Coach
    {
        [Key]
        public Guid UserId { get; set; }

        public Guid SportId { get; set; }
        public string Bio { get; set; } = string.Empty;
        public decimal RatePerHour { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}