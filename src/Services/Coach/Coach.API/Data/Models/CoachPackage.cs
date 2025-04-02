using System.ComponentModel.DataAnnotations;

namespace Coach.API.Data.Models
{
    public class CoachPackage
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CoachId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int SessionCount { get; set; }
        public string Status { get; set; } = "active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual Coach Coach { get; set; }
        public virtual ICollection<CoachBooking> Bookings { get; set; }
        public virtual ICollection<CoachPackagePurchase> Purchases { get; set; }
        public ICollection<CoachPromotion> Promotions { get; set; }
    }
}