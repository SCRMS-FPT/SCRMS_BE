using System.ComponentModel.DataAnnotations;

namespace Matching.API.Data.Models
{
    public class Match
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid InitiatorId { get; set; }

        [Required]
        public Guid MatchedUserId { get; set; }

        [Required]
        public DateTime MatchTime { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "confirmed";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}