using Payment.API.Data.Repositories;

namespace Payment.API.Features.SePay
{
    public record ProcessSepayWebhookCommand(SepayWebhookRequest Request) : IRequest<bool>;

    public class ProcessSepayWebhookHandler : IRequestHandler<ProcessSepayWebhookCommand, bool>
    {
        private readonly IUserWalletRepository _userWalletRepo;
        private readonly IWalletTransactionRepository _transactionRepo;
        private readonly PaymentDbContext _context;

        public ProcessSepayWebhookHandler(IUserWalletRepository userWalletRepo, IWalletTransactionRepository transactionRepo, PaymentDbContext context)
        {
            _userWalletRepo = userWalletRepo;
            _transactionRepo = transactionRepo;
            _context = context;
        }

        public async Task<bool> Handle(ProcessSepayWebhookCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Chỉ xử lý giao dịch tiền vào
            if (request.TransferType != "in")
                return false;

            // Lấy userId từ nội dung
            string content = request.Content.Trim();

            // Kiểm tra nếu nội dung là một GUID hợp lệ
            if (!Guid.TryParse(content, out Guid userId))
                return false;

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var wallet = await _userWalletRepo.GetUserWalletByUserIdAsync(userId, cancellationToken);
                if (wallet == null)
                {
                    // Tạo ví mới nếu chưa có
                    wallet = new UserWallet
                    {
                        UserId = userId,
                        Balance = 0,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _userWalletRepo.AddUserWalletAsync(wallet, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                // Cập nhật số dư ví
                wallet.Balance += request.TransferAmount;
                wallet.UpdatedAt = DateTime.UtcNow;
                await _userWalletRepo.UpdateUserWalletAsync(wallet, cancellationToken);

                // Lưu giao dịch vào lịch sử
                var transactionRecord = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    TransactionType = "deposit",
                    ReferenceId = Guid.Parse(request.ReferenceCode),
                    Amount = request.TransferAmount,
                    Description = "Nạp tiền qua chuyển khoản",
                    CreatedAt = DateTime.UtcNow
                };

                await _transactionRepo.AddWalletTransactionAsync(transactionRecord, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return true;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }
        }
    }
}
