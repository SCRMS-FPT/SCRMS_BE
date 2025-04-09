using Microsoft.EntityFrameworkCore;
using Payment.API.Data.Repositories;
using Payment.API.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Payment.API.Features.GetUserWithdrawalRequests
{
    public record GetUserWithdrawalRequestsQuery(Guid UserId) : IRequest<List<WithdrawalRequest>>;

    public class GetUserWithdrawalRequestsHandler : IRequestHandler<GetUserWithdrawalRequestsQuery, List<WithdrawalRequest>>
    {
        private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;

        public GetUserWithdrawalRequestsHandler(IWithdrawalRequestRepository withdrawalRequestRepository)
        {
            _withdrawalRequestRepository = withdrawalRequestRepository;
        }

        public async Task<List<WithdrawalRequest>> Handle(GetUserWithdrawalRequestsQuery request, CancellationToken cancellationToken)
        {
            return await _withdrawalRequestRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        }
    }
}