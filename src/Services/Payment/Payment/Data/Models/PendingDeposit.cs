using System;

namespace Payment.API.Data.Models
{
    public class PendingDeposit
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } // Pending, Completed
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}