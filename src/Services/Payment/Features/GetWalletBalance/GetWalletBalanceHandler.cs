using Microsoft.EntityFrameworkCore;

namespace Payment.API.Features.GetWalletBalance
{
    public record GetWalletBalanceQuery(Guid UserId) : IRequest<UserWallet>;

    public class GetWalletBalanceHandler : IRequestHandler<GetWalletBalanceQuery, UserWallet>
    {
        private readonly PaymentDbContext _context;

        public GetWalletBalanceHandler(PaymentDbContext context) => _context = context;

        public async Task<UserWallet> Handle(GetWalletBalanceQuery request, CancellationToken cancellationToken)
        {
            var wallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);
            if (wallet == null)
                throw new Exception("Wallet not found");

            return wallet;
        }
    }
}