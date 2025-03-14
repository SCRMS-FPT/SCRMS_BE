namespace Coach.API.Data.Models
{
    public class CoachPackagePurchase
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CoachPackageId { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
        public int SessionsUsed { get; set; } = 0;
        public DateTime ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public CoachPackage CoachPackage { get; set; }
    }
}