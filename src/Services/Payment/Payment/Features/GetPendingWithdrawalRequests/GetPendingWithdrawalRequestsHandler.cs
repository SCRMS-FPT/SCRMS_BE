using Microsoft.EntityFrameworkCore;
using Payment.API.Data.Repositories;
using Payment.API.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Pagination;

namespace Payment.API.Features.GetPendingWithdrawalRequests
{
    public record GetPendingWithdrawalRequestsQuery(int Page, int Limit) : IRequest<PaginatedResult<WithdrawalRequest>>;

    public class GetPendingWithdrawalRequestsHandler : IRequestHandler<GetPendingWithdrawalRequestsQuery, PaginatedResult<WithdrawalRequest>>
    {
        private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
        private readonly PaymentDbContext _context;

        public GetPendingWithdrawalRequestsHandler(IWithdrawalRequestRepository withdrawalRequestRepository, PaymentDbContext context)
        {
            _withdrawalRequestRepository = withdrawalRequestRepository;
            _context = context;
        }

        public async Task<PaginatedResult<WithdrawalRequest>> Handle(GetPendingWithdrawalRequestsQuery request, CancellationToken cancellationToken)
        {
            var pendingRequests = await _withdrawalRequestRepository.GetPendingRequestsAsync(request.Page, request.Limit, cancellationToken);
            var totalCount = await _context.WithdrawalRequests.CountAsync(w => w.Status == "Pending", cancellationToken);

            return new PaginatedResult<WithdrawalRequest>(
                request.Page,
                request.Limit,
                totalCount,
                pendingRequests
            );
        }
    }
}