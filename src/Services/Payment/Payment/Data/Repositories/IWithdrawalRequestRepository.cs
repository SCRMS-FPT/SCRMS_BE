using Payment.API.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Payment.API.Data.Repositories
{
    public interface IWithdrawalRequestRepository
    {
        Task<WithdrawalRequest> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<List<WithdrawalRequest>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<List<WithdrawalRequest>> GetPendingRequestsAsync(int page, int limit, CancellationToken cancellationToken);
        Task AddAsync(WithdrawalRequest request, CancellationToken cancellationToken);
        Task UpdateAsync(WithdrawalRequest request, CancellationToken cancellationToken);
    }
}