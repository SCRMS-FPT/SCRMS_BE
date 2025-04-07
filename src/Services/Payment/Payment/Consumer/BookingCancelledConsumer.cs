using BuildingBlocks.Messaging.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore.Storage;
using Payment.API.Data.Repositories;

namespace Payment.API.Consumer
{
    // Payment.API/Features/ProcessRefund/ProcessRefundConsumer.cs
    public class BookingCancelledConsumer : IConsumer<BookingCancelledRefundEvent>
    {
        private readonly IUserWalletRepository _userWalletRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BookingCancelledConsumer> _logger;

        public BookingCancelledConsumer(
            IUserWalletRepository userWalletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            IUnitOfWork unitOfWork,
            ILogger<BookingCancelledConsumer> logger)
        {
            _userWalletRepository = userWalletRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingCancelledRefundEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation($"Processing refund for cancelled booking {message.BookingId} - Refund amount: {message.RefundAmount}");

            // Begin a transaction to ensure both wallets are updated atomically
            using var transaction = await _unitOfWork.BeginTransactionAsync(context.CancellationToken);

            try
            {
                // Get user's wallet
                var userWallet = await _userWalletRepository.GetUserWalletByUserIdAsync(message.UserId, context.CancellationToken);
                if (userWallet == null)
                {
                    // Create wallet if it doesn't exist
                    userWallet = new UserWallet
                    {
                        UserId = message.UserId,
                        Balance = 0,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _userWalletRepository.AddUserWalletAsync(userWallet, context.CancellationToken);
                }

                // Update user's wallet balance
                userWallet.Balance += message.RefundAmount;
                userWallet.UpdatedAt = DateTime.UtcNow;
                await _userWalletRepository.UpdateUserWalletAsync(userWallet, context.CancellationToken);

                // Record refund transaction for user
                var userTransaction = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = message.UserId,
                    TransactionType = "REFUND",
                    ReferenceId = message.BookingId,
                    Amount = message.RefundAmount,
                    Description = $"Refund for cancelled booking {message.BookingId}: {message.CancellationReason}",
                    CreatedAt = DateTime.UtcNow
                };
                await _walletTransactionRepository.AddWalletTransactionAsync(userTransaction, context.CancellationToken);

                // Get sport center owner's wallet
                var ownerWallet = await _userWalletRepository.GetUserWalletByUserIdAsync(message.SportCenterOwnerId, context.CancellationToken);
                if (ownerWallet == null)
                {
                    // Create wallet if it doesn't exist
                    ownerWallet = new UserWallet
                    {
                        UserId = message.SportCenterOwnerId,
                        Balance = 0,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _userWalletRepository.AddUserWalletAsync(ownerWallet, context.CancellationToken);
                }

                // Check if owner has enough balance
                if (ownerWallet.Balance < message.RefundAmount)
                {
                    _logger.LogWarning($"Sport center owner {message.SportCenterOwnerId} doesn't have enough balance for refund. Current balance: {ownerWallet.Balance}, Refund amount: {message.RefundAmount}");
                    // Continue anyway - we might want to handle this differently in a real system
                }

                // Deduct from owner's wallet
                ownerWallet.Balance -= message.RefundAmount;
                ownerWallet.UpdatedAt = DateTime.UtcNow;
                await _userWalletRepository.UpdateUserWalletAsync(ownerWallet, context.CancellationToken);

                // Record refund deduction transaction for owner
                var ownerTransaction = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = message.SportCenterOwnerId,
                    TransactionType = "REFUND_DEDUCTION",
                    ReferenceId = message.BookingId,
                    Amount = -message.RefundAmount,  // Negative amount for deduction
                    Description = $"Refund deduction for cancelled booking {message.BookingId}: {message.CancellationReason}",
                    CreatedAt = DateTime.UtcNow
                };
                await _walletTransactionRepository.AddWalletTransactionAsync(ownerTransaction, context.CancellationToken);

                await _unitOfWork.SaveChangesAsync(context.CancellationToken);
                await transaction.CommitAsync(context.CancellationToken);

                // Publish refund completed event
                await context.Publish(new RefundProcessedEvent(
                    userTransaction.Id,
                    message.UserId,
                    message.RefundAmount,
                    DateTime.UtcNow,
                    $"Refund for cancelled booking {message.BookingId}"
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(context.CancellationToken);
                _logger.LogError(ex, $"Error processing refund for booking {message.BookingId}");
                throw;
            }
        }
    }
}