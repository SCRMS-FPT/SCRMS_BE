using Microsoft.EntityFrameworkCore;
using Payment.API.Data.Repositories;

namespace Payment.API.Features.GetTransactionHistory
{
    public record GetTransactionHistoryQuery(Guid UserId, int Page, int Limit) : IRequest<List<WalletTransaction>>;

    public class GetTransactionHistoryHandler : IRequestHandler<GetTransactionHistoryQuery, List<WalletTransaction>>
    {
        private readonly IWalletTransactionRepository _walletTransactionRepository;

        public GetTransactionHistoryHandler(IWalletTransactionRepository walletTransactionRepository)
        {
            _walletTransactionRepository = walletTransactionRepository;
        }

        public async Task<List<WalletTransaction>> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
        {
            return await _walletTransactionRepository.GetTransactionsByUserIdAsync(request.UserId, request.Page, request.Limit, cancellationToken);
        }
    }
}