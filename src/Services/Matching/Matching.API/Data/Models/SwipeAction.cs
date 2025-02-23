using System.ComponentModel.DataAnnotations;

namespace Matching.API.Data.Models
{
    public class SwipeAction
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid SwiperId { get; set; }

        [Required]
        public Guid SwipedUserId { get; set; }

        [Required]
        [MaxLength(10)]
        public string Decision { get; set; }  // pending, accepted, reject

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}