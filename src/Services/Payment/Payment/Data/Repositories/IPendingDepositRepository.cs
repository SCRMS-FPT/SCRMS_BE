using Payment.API.Data.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Payment.API.Data.Repositories
{
    public interface IPendingDepositRepository
    {
        Task<PendingDeposit> GetPendingDepositByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<PendingDeposit> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(PendingDeposit pendingDeposit, CancellationToken cancellationToken = default);
        Task UpdateAsync(PendingDeposit pendingDeposit, CancellationToken cancellationToken = default);
    }
}