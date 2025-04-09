using System;
using System.ComponentModel.DataAnnotations;

namespace Payment.API.Data.Models
{
    public class WithdrawalRequest
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolderName { get; set; }
        public string Status { get; set; } // "Pending", "Approved", "Rejected"
        public string AdminNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public Guid? ProcessedByUserId { get; set; }
    }
}