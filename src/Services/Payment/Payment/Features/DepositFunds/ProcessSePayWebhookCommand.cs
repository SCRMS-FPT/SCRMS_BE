using MediatR;
using System;

namespace Payment.API.Features.DepositFunds
{
    public record ProcessSePayWebhookCommand(SePayWebhookModel WebhookData) : IRequest<ProcessSePayWebhookResult>;

    public class ProcessSePayWebhookResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Guid? TransactionId { get; set; }
    }

    public class SePayWebhookModel
    {
        public long id { get; set; }
        public string gateway { get; set; }
        public string transactionDate { get; set; }
        public string accountNumber { get; set; }
        public string code { get; set; }
        public string content { get; set; }
        public string transferType { get; set; }
        public decimal transferAmount { get; set; }
        public decimal accumulated { get; set; }
        public string subAccount { get; set; }
        public string referenceCode { get; set; }
        public string description { get; set; }
    }
}