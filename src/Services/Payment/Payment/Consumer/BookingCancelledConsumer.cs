using BuildingBlocks.Messaging.Events;
using MassTransit;
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
            _logger.LogInformation($"Processing refund for cancelled booking {message.BookingId}");

            // Get user wallet
            var wallet = await _userWalletRepository.GetUserWalletByUserIdAsync(message.UserId, context.CancellationToken);
            if (wallet == null)
            {
                // Create wallet if it doesn't exist
                wallet = new UserWallet
                {
                    UserId = message.UserId,
                    Balance = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                await _userWalletRepository.AddUserWalletAsync(wallet, context.CancellationToken);
            }

            // Update wallet balance
            wallet.Balance += message.RefundAmount;
            wallet.UpdatedAt = DateTime.UtcNow;
            await _userWalletRepository.UpdateUserWalletAsync(wallet, context.CancellationToken);

            // Record transaction
            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                UserId = message.UserId,
                TransactionType = "REFUND",
                ReferenceId = message.BookingId,
                Amount = message.RefundAmount,
                Description = $"Refund for cancelled booking {message.BookingId}: {message.CancellationReason}",
                CreatedAt = DateTime.UtcNow
            };

            await _walletTransactionRepository.AddWalletTransactionAsync(transaction, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            // Publish refund completed event
            await context.Publish(new RefundProcessedEvent(
                transaction.Id,
                message.UserId,
                message.RefundAmount,
                DateTime.UtcNow,
                $"Refund for cancelled booking {message.BookingId}"
            ));
        }
    }
}