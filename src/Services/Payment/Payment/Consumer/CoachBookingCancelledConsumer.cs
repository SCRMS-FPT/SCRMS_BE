using BuildingBlocks.Messaging.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore.Storage;
using Payment.API.Data.Repositories;

namespace Payment.API.Consumer
{
    public class CoachBookingCancelledConsumer : IConsumer<CoachBookingCancelledRefundEvent>
    {
        private readonly IUserWalletRepository _userWalletRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CoachBookingCancelledConsumer> _logger;

        public CoachBookingCancelledConsumer(
            IUserWalletRepository userWalletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            IUnitOfWork unitOfWork,
            ILogger<CoachBookingCancelledConsumer> logger)
        {
            _userWalletRepository = userWalletRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CoachBookingCancelledRefundEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation($"Processing refund for cancelled coach booking {message.BookingId} - Refund amount: {message.RefundAmount}");

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
                    TransactionType = "COACH_BOOKING_REFUND",
                    ReferenceId = message.BookingId,
                    Amount = message.RefundAmount,
                    Description = $"Refund for cancelled coach booking {message.BookingId}: {message.CancellationReason}",
                    CreatedAt = DateTime.UtcNow
                };
                await _walletTransactionRepository.AddWalletTransactionAsync(userTransaction, context.CancellationToken);

                // Get coach's wallet
                var coachWallet = await _userWalletRepository.GetUserWalletByUserIdAsync(message.CoachId, context.CancellationToken);
                if (coachWallet == null)
                {
                    // Create wallet if it doesn't exist
                    coachWallet = new UserWallet
                    {
                        UserId = message.CoachId,
                        Balance = 0,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _userWalletRepository.AddUserWalletAsync(coachWallet, context.CancellationToken);
                }

                // Check if coach has enough balance
                if (coachWallet.Balance < message.RefundAmount)
                {
                    _logger.LogWarning($"Coach {message.CoachId} doesn't have enough balance for refund. Current balance: {coachWallet.Balance}, Refund amount: {message.RefundAmount}");
                    // Continue anyway - we might want to handle this differently in a real system
                }

                // Deduct from coach's wallet
                coachWallet.Balance -= message.RefundAmount;
                coachWallet.UpdatedAt = DateTime.UtcNow;
                await _userWalletRepository.UpdateUserWalletAsync(coachWallet, context.CancellationToken);

                // Record refund deduction transaction for coach
                var coachTransaction = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = message.CoachId,
                    TransactionType = "COACH_REFUND_DEDUCTION",
                    ReferenceId = message.BookingId,
                    Amount = -message.RefundAmount,  // Negative amount for deduction
                    Description = $"Refund deduction for cancelled coach booking {message.BookingId}: {message.CancellationReason}",
                    CreatedAt = DateTime.UtcNow
                };
                await _walletTransactionRepository.AddWalletTransactionAsync(coachTransaction, context.CancellationToken);

                await _unitOfWork.SaveChangesAsync(context.CancellationToken);
                await transaction.CommitAsync(context.CancellationToken);

                // Publish refund completed event
                await context.Publish(new RefundProcessedEvent(
                    userTransaction.Id,
                    message.UserId,
                    message.RefundAmount,
                    DateTime.UtcNow,
                    $"Refund for cancelled coach booking {message.BookingId}"
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(context.CancellationToken);
                _logger.LogError(ex, $"Error processing refund for coach booking {message.BookingId}");
                throw;
            }
        }
    }
}
