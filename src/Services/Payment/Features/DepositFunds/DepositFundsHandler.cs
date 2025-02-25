namespace Payment.API.Features.DepositFunds
{
    public record DepositFundsCommand(Guid UserId, decimal Amount, string TransactionReference) : IRequest<Guid>;

    public class DepositFundsHandler : IRequestHandler<DepositFundsCommand, Guid>
    {
        private readonly PaymentDbContext _context;

        public DepositFundsHandler(PaymentDbContext context) => _context = context;

        public async Task<Guid> Handle(DepositFundsCommand request, CancellationToken cancellationToken)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be positive.");

            var wallet = await _context.UserWallets.FindAsync(request.UserId);
            if (wallet == null)
            {
                wallet = new UserWallet { UserId = request.UserId, Balance = 0, UpdatedAt = DateTime.UtcNow };
                _context.UserWallets.Add(wallet);
            }

            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                TransactionType = "deposit",
                Amount = request.Amount,
                Description = $"Deposit via payment gateway, Ref: {request.TransactionReference}",
                CreatedAt = DateTime.UtcNow
            };

            wallet.Balance += request.Amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);

            return transaction.Id;
        }
    }
}