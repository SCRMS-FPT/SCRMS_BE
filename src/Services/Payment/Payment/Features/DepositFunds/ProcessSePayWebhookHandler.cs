using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.Outbox;
using Payment.API.Data.Repositories;
using System.Text.RegularExpressions;

namespace Payment.API.Features.DepositFunds
{
    public class ProcessSePayWebhookHandler : IRequestHandler<ProcessSePayWebhookCommand, ProcessSePayWebhookResult>
    {
        private readonly IPendingDepositRepository _pendingDepositRepository;
        private readonly IUserWalletRepository _userWalletRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IOutboxService _outboxService;
        private readonly ILogger<ProcessSePayWebhookHandler> _logger;

        public ProcessSePayWebhookHandler(
            IPendingDepositRepository pendingDepositRepository,
            IUserWalletRepository userWalletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            IOutboxService outboxService,
            ILogger<ProcessSePayWebhookHandler> logger)
        {
            _pendingDepositRepository = pendingDepositRepository;
            _userWalletRepository = userWalletRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _outboxService = outboxService;
            _logger = logger;
        }

        public async Task<ProcessSePayWebhookResult> Handle(ProcessSePayWebhookCommand request, CancellationToken cancellationToken)
        {
            var webhook = request.WebhookData;

            // Only process incoming transfers
            if (webhook.transferType != "in")
            {
                _logger.LogInformation($"Ignoring outgoing transfer with ID {webhook.id}");
                return new ProcessSePayWebhookResult { Success = false, Message = "Not an incoming transfer." };
            }

            // Extract the deposit code from the content
            var code = webhook.code ?? ExtractCodeFromContent(webhook.content);
            if (string.IsNullOrEmpty(code))
            {
                _logger.LogWarning($"No deposit code found in content: '{webhook.content}'");
                return new ProcessSePayWebhookResult { Success = false, Message = "No code found in content." };
            }

            // Find the pending deposit
            var pendingDeposit = await _pendingDepositRepository.GetPendingDepositByCodeAsync(code, cancellationToken);
            if (pendingDeposit == null)
            {
                _logger.LogWarning($"No pending deposit found for code: {code}");
                return new ProcessSePayWebhookResult { Success = false, Message = "No matching pending deposit." };
            }

            // Check if deposit is already processed
            if (pendingDeposit.Status != "Pending")
            {
                _logger.LogWarning($"Deposit {pendingDeposit.Id} with code {code} is already {pendingDeposit.Status}");
                return new ProcessSePayWebhookResult { Success = false, Message = $"Deposit is already {pendingDeposit.Status}." };
            }

            // Verify the amount matches (allow a small tolerance for rounding errors)
            const decimal tolerance = 0.01m;
            if (Math.Abs(pendingDeposit.Amount - webhook.transferAmount) > tolerance)
            {
                _logger.LogWarning($"Amount mismatch for deposit {pendingDeposit.Id}. Expected: {pendingDeposit.Amount}, Received: {webhook.transferAmount}");
                return new ProcessSePayWebhookResult { Success = false, Message = $"Amount mismatch. Expected: {pendingDeposit.Amount}, Received: {webhook.transferAmount}" };
            }

            try
            {
                // Get or create user wallet
                var wallet = await _userWalletRepository.GetUserWalletByUserIdAsync(pendingDeposit.UserId, cancellationToken);
                if (wallet == null)
                {
                    wallet = new UserWallet
                    {
                        UserId = pendingDeposit.UserId,
                        Balance = 0,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _userWalletRepository.AddUserWalletAsync(wallet, cancellationToken);
                    _logger.LogInformation($"Created new wallet for user {pendingDeposit.UserId}");
                }

                // Credit user's wallet
                wallet.Balance += pendingDeposit.Amount;
                wallet.UpdatedAt = DateTime.UtcNow;
                await _userWalletRepository.UpdateUserWalletAsync(wallet, cancellationToken);
                _logger.LogInformation($"Credited {pendingDeposit.Amount} to wallet for user {pendingDeposit.UserId}. New balance: {wallet.Balance}");

                // Create a transaction record
                var transaction = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = pendingDeposit.UserId,
                    Amount = pendingDeposit.Amount,
                    Description = pendingDeposit.Description ?? "Deposit via bank transfer",
                    TransactionType = "Deposit",
                    CreatedAt = DateTime.UtcNow,
                    ReferenceId = pendingDeposit.Id
                };
                await _walletTransactionRepository.AddWalletTransactionAsync(transaction, cancellationToken);
                _logger.LogInformation($"Created transaction record {transaction.Id} for deposit {pendingDeposit.Id}");

                // Mark deposit as completed
                pendingDeposit.Status = "Completed";
                pendingDeposit.CompletedAt = DateTime.UtcNow;
                await _pendingDepositRepository.UpdateAsync(pendingDeposit, cancellationToken);
                _logger.LogInformation($"Marked deposit {pendingDeposit.Id} as Completed");

                // Create payment event
                var paymentEvent = new PaymentSucceededEvent(
                    transaction.Id,
                    pendingDeposit.UserId,
                    null,
                    pendingDeposit.Amount,
                    DateTime.UtcNow,
                    pendingDeposit.Description ?? "Deposit via bank transfer",
                    "Deposit"
                );

                // Save to outbox
                await _outboxService.SaveMessageAsync(paymentEvent);

                return new ProcessSePayWebhookResult
                {
                    Success = true,
                    TransactionId = transaction.Id,
                    Message = "Deposit processed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing deposit {pendingDeposit.Id}: {ex.Message}");
                return new ProcessSePayWebhookResult
                {
                    Success = false,
                    Message = $"Error processing deposit: {ex.Message}"
                };
            }
        }

        private string ExtractCodeFromContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            // Look for ORD followed by digits in the content
            var match = Regex.Match(content, @"ORD\d+");
            if (match.Success)
            {
                return match.Value;
            }

            return null;
        }
    }
}