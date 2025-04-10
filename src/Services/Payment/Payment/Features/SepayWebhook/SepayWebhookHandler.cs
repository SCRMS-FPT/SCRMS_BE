using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Payment.API.Data.Repositories;
using Payment.API.Data;
using Microsoft.EntityFrameworkCore;

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

            // Kiểm tra giao dịch trùng lặp
            var existingTransaction = await _transactionRepo.GetByReferenceCodeAsync(Guid.Parse(request.ReferenceCode));
            if (existingTransaction != null)
                return true; // Đã xử lý trước đó

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                string formattedGuid = $"{request.Content.Substring(0, 8)}-" +
                       $"{request.Content.Substring(8, 4)}-" +
                       $"{request.Content.Substring(12, 4)}-" +
                       $"{request.Content.Substring(16, 4)}-" +
                       $"{request.Content.Substring(20, 12)}";

                var wallet = await _userWalletRepo.GetUserWalletByUserIdAsync(Guid.Parse(formattedGuid), cancellationToken);
                if (wallet == null)
                    return false;

                // Cập nhật số dư ví
                wallet.Balance += request.TransferAmount;
                wallet.UpdatedAt = DateTime.UtcNow;
                await _userWalletRepo.UpdateUserWalletAsync(wallet, cancellationToken);

                // Lưu giao dịch vào lịch sử
                var transactionRecord = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = wallet.UserId,
                    TransactionType = "deposit",
                    ReferenceId = Guid.Parse(request.ReferenceCode),
                    Amount = request.TransferAmount,
                    Description = request.Content,
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
