using Microsoft.EntityFrameworkCore;
using Payment.API.Data.Repositories;
using BuildingBlocks.Pagination;

namespace Payment.API.Features.GetTransactionHistory
{
    public record GetTransactionHistoryQuery(Guid UserId, int Page, int Limit) : IRequest<PaginatedResult<WalletTransaction>>;

    public class GetTransactionHistoryHandler : IRequestHandler<GetTransactionHistoryQuery, PaginatedResult<WalletTransaction>>
    {
        private readonly IWalletTransactionRepository _walletTransactionRepository;

        public GetTransactionHistoryHandler(IWalletTransactionRepository walletTransactionRepository)
        {
            _walletTransactionRepository = walletTransactionRepository;
        }

        public async Task<PaginatedResult<WalletTransaction>> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
        {
            // Get total count
            var totalCount = await _walletTransactionRepository.GetTransactionCountByUserIdAsync(request.UserId, cancellationToken);

            // Get paginated data
            var transactions = await _walletTransactionRepository.GetTransactionsByUserIdAsync(
                request.UserId,
                request.Page,
                request.Limit,
                cancellationToken);

            // Return the paginated result
            return new PaginatedResult<WalletTransaction>(
                pageIndex: request.Page,
                pageSize: request.Limit,
                count: totalCount,
                data: transactions
            );
        }
    }
}