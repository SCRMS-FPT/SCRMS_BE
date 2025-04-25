using MediatR;
using Payment.API.Data.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Payment.API.Features.DepositFunds
{
    public record CheckDepositStatusQuery(Guid Id) : IRequest<CheckDepositStatusResult?>;

    public class CheckDepositStatusResult
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class CheckDepositStatusHandler : IRequestHandler<CheckDepositStatusQuery, CheckDepositStatusResult?>
    {
        private readonly IPendingDepositRepository _pendingDepositRepository;

        public CheckDepositStatusHandler(IPendingDepositRepository pendingDepositRepository)
        {
            _pendingDepositRepository = pendingDepositRepository;
        }

        public async Task<CheckDepositStatusResult?> Handle(CheckDepositStatusQuery request, CancellationToken cancellationToken)
        {
            var deposit = await _pendingDepositRepository.GetByIdAsync(request.Id, cancellationToken);
            if (deposit == null)
                return null;

            return new CheckDepositStatusResult
            {
                Id = deposit.Id,
                Code = deposit.Code,
                Amount = deposit.Amount,
                Status = deposit.Status,
                CompletedAt = deposit.CompletedAt
            };
        }
    }
}