namespace Payment.API.Data.Models
{
    public class UserWallet
    {
        public Guid UserId { get; set; }
        public decimal Balance { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}