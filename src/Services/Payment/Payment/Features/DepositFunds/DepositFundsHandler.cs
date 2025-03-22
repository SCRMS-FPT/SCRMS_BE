using MassTransit;
using Payment.API.Data.Repositories;
using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.Outbox;
using Payment.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Payment.API.Features.DepositFunds
{
    public record DepositFundsCommand(
        Guid UserId,
        decimal Amount,
        string Description) : IRequest<Guid>;

    public class DepositFundsHandler : IRequestHandler<DepositFundsCommand, Guid>
    {
        private readonly IUserWalletRepository _userWalletRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly PaymentDbContext _context;
        private readonly IOutboxService _outboxService;

        public DepositFundsHandler(
            IUserWalletRepository userWalletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            PaymentDbContext context,
            IOutboxService outboxService)
        {
            _userWalletRepository = userWalletRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _context = context;
            _outboxService = outboxService;
        }

        public async Task<Guid> Handle(DepositFundsCommand request, CancellationToken cancellationToken)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be positive.");

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Get or create wallet
                var wallet = await _userWalletRepository.GetUserWalletByUserIdAsync(request.UserId, cancellationToken);
                bool isNewWallet = false;

                if (wallet == null)
                {
                    isNewWallet = true;
                    wallet = new UserWallet
                    {
                        UserId = request.UserId,
                        Balance = 0,
                        UpdatedAt = DateTime.UtcNow
                    };
                }

                // Create transaction record
                var transactionRecord = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    TransactionType = "deposit",
                    ReferenceId = null,
                    Amount = request.Amount,
                    Description = request.Description ?? "Deposit funds",
                    CreatedAt = DateTime.UtcNow
                };

                // Update wallet balance
                wallet.Balance += request.Amount;
                wallet.UpdatedAt = DateTime.UtcNow;

                // Save changes
                if (isNewWallet)
                {
                    await _userWalletRepository.AddUserWalletAsync(wallet, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    await _userWalletRepository.UpdateUserWalletAsync(wallet, cancellationToken);
                }

                await _walletTransactionRepository.AddWalletTransactionAsync(transactionRecord, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                // Create simple deposit event
                var paymentEvent = new PaymentSucceededEvent(
                    transactionRecord.Id,
                    request.UserId,
                    null,
                    request.Amount,
                    DateTime.UtcNow,
                    request.Description ?? "Wallet deposit",
                    "Deposit"
                );

                // Save to outbox
                await _outboxService.SaveMessageAsync(paymentEvent);

                await transaction.CommitAsync(cancellationToken);
                return transactionRecord.Id;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    public class GetUserWalletQueryHandler : IRequestHandler<GetUserWalletQuery, UserWallet>
    {
        private readonly IUserWalletRepository _userWalletRepository;

        public GetUserWalletQueryHandler(IUserWalletRepository userWalletRepository)
        {
            _userWalletRepository = userWalletRepository;
        }

        public async Task<UserWallet> Handle(GetUserWalletQuery request, CancellationToken cancellationToken)
        {
            return await _userWalletRepository.GetUserWalletByUserIdAsync(request.UserId, cancellationToken);
        }
    }
}