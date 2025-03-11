using MassTransit;
using Payment.API.Data.Repositories;
using BuildingBlocks.Messaging.Events;

namespace Payment.API.Features.DepositFunds
{
    public record DepositFundsCommand(Guid UserId, decimal Amount, string TransactionReference) : IRequest<Guid>;

    public class DepositFundsHandler : IRequestHandler<DepositFundsCommand, Guid>
    {
        private readonly IUserWalletRepository _userWalletRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly PaymentDbContext _context;

        public DepositFundsHandler(
            IUserWalletRepository userWalletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            PaymentDbContext context)
        {
            _userWalletRepository = userWalletRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _context = context;
        }

        public async Task<Guid> Handle(DepositFundsCommand request, CancellationToken cancellationToken)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be positive.");

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var wallet = await _userWalletRepository.GetUserWalletByUserIdAsync(request.UserId, cancellationToken);
                if (wallet == null)
                {
                    wallet = new UserWallet { UserId = request.UserId, Balance = 0, UpdatedAt = DateTime.UtcNow };
                    await _userWalletRepository.AddUserWalletAsync(wallet, cancellationToken);
                }

                var transactionRecord = new WalletTransaction
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

                await _walletTransactionRepository.AddWalletTransactionAsync(transactionRecord, cancellationToken);
                await _userWalletRepository.UpdateUserWalletAsync(wallet, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // Publish event after successful commit
                await _publishEndpoint.Publish(new PaymentSucceededEvent(
                    transactionRecord.Id,
                    request.UserId,
                    request.Amount,
                    DateTime.UtcNow,
                    "Deposit succeeded"
                ));

                return transactionRecord.Id;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}