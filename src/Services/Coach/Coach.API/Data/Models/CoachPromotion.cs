namespace Coach.API.Data.Models
{
    public class CoachPromotion
    {
        public Guid Id { get; set; } // UUID, PK
        public Guid CoachId { get; set; } // UUID, FK tới coaches(user_id)
        public Guid? PackageId { get; set; }
        public string Description { get; set; } // TEXT, NULLABLE
        public string DiscountType { get; set; } // VARCHAR(50)
        public decimal DiscountValue { get; set; } // DECIMAL
        public DateOnly ValidFrom { get; set; } // DATE
        public DateOnly ValidTo { get; set; } // DATE
        public DateTime CreatedAt { get; set; } = DateTime.Now; // TIMESTAMP, DEFAULT NOW()
        public DateTime UpdatedAt { get; set; } = DateTime.Now; // TIMESTAMP, DEFAULT NOW()

        // Navigation property
        public virtual Coach Coach { get; set; }
        public virtual CoachPackage Package { get; set; }
    }
}