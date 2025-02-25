using Microsoft.EntityFrameworkCore;

namespace Payment.API.Features.GetTransactionHistory
{
    public record GetTransactionHistoryQuery(Guid UserId, int Page, int Limit) : IRequest<List<WalletTransaction>>;

    public class GetTransactionHistoryHandler : IRequestHandler<GetTransactionHistoryQuery, List<WalletTransaction>>
    {
        private readonly PaymentDbContext _context;

        public GetTransactionHistoryHandler(PaymentDbContext context) => _context = context;

        public async Task<List<WalletTransaction>> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
        {
            return await _context.WalletTransactions
                .Where(t => t.UserId == request.UserId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((request.Page - 1) * request.Limit)
                .Take(request.Limit)
                .ToListAsync(cancellationToken);
        }
    }
}