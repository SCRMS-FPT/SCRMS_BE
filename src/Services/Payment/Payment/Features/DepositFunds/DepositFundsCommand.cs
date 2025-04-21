using MediatR;
using System;

namespace Payment.API.Features.DepositFunds
{
    public record DepositFundsCommand(
        Guid UserId,
        decimal Amount,
        string Description) : IRequest<DepositFundsResult>;

    public class DepositFundsResult
    {
        public Guid DepositId { get; set; }
        public string DepositCode { get; set; }
        public decimal Amount { get; set; }
        public string BankInfo { get; set; }
    }
}