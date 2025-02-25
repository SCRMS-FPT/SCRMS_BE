using Microsoft.EntityFrameworkCore;

namespace Payment.API.Features.ProcessBookingPayment
{
    public record ProcessBookingPaymentCommand(Guid UserId, decimal Amount, string Description) : IRequest<Guid>;

    public class ProcessBookingPaymentHandler : IRequestHandler<ProcessBookingPaymentCommand, Guid>
    {
        private readonly PaymentDbContext _context;

        public ProcessBookingPaymentHandler(PaymentDbContext context) => _context = context;

        public async Task<Guid> Handle(ProcessBookingPaymentCommand request, CancellationToken cancellationToken)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be positive.");

            var wallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);
            if (wallet == null || wallet.Balance < request.Amount)
                throw new Exception("Insufficient balance.");

            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                TransactionType = "booking",
                Amount = -request.Amount,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            wallet.Balance -= request.Amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);

            return transaction.Id;
        }
    }
}