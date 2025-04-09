using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payment.API.Data.Repositories;
using Payment.API.Data;
using Payment.API.Data.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Messaging.Outbox;
using BuildingBlocks.Messaging.Events;

namespace Payment.API.Features.ProcessWithdrawalRequest
{
    public record ProcessWithdrawalRequestCommand(
        Guid RequestId,
        string Status,
        string AdminNote,
        Guid AdminUserId) : IRequest<bool>;

    public class ProcessWithdrawalRequestHandler : IRequestHandler<ProcessWithdrawalRequestCommand, bool>
    {
        private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
        private readonly IUserWalletRepository _userWalletRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOutboxService _outboxService;

        public ProcessWithdrawalRequestHandler(
            IWithdrawalRequestRepository withdrawalRequestRepository,
            IUserWalletRepository userWalletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            IUnitOfWork unitOfWork,
            IOutboxService outboxService)
        {
            _withdrawalRequestRepository = withdrawalRequestRepository;
            _userWalletRepository = userWalletRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _unitOfWork = unitOfWork;
            _outboxService = outboxService;
        }

        public async Task<bool> Handle(ProcessWithdrawalRequestCommand request, CancellationToken cancellationToken)
        {
            var withdrawalRequest = await _withdrawalRequestRepository.GetByIdAsync(request.RequestId, cancellationToken);
            if (withdrawalRequest == null)
                throw new ArgumentException("Withdrawal request not found");

            if (withdrawalRequest.Status != "Pending")
                throw new InvalidOperationException("Only pending withdrawal requests can be processed");

            // Cập nhật thông tin yêu cầu rút tiền
            withdrawalRequest.Status = request.Status;
            withdrawalRequest.AdminNote = request.AdminNote;
            withdrawalRequest.ProcessedAt = DateTime.UtcNow;
            withdrawalRequest.ProcessedByUserId = request.AdminUserId;

            // Đảm bảo CreatedAt luôn là UTC
            withdrawalRequest.CreatedAt = DateTime.SpecifyKind(withdrawalRequest.CreatedAt, DateTimeKind.Utc);

            await _withdrawalRequestRepository.UpdateAsync(withdrawalRequest, cancellationToken);

            // Nếu phê duyệt, cập nhật số dư ví và tạo giao dịch
            if (request.Status == "Approved")
            {
                var wallet = await _userWalletRepository.GetUserWalletByUserIdAsync(withdrawalRequest.UserId, cancellationToken);
                if (wallet == null)
                    throw new InvalidOperationException("User wallet not found");

                // Kiểm tra số dư lại lần nữa
                if (wallet.Balance < withdrawalRequest.Amount)
                    throw new InvalidOperationException("Insufficient funds in wallet");

                // Trừ tiền khỏi ví
                wallet.Balance -= withdrawalRequest.Amount;
                wallet.UpdatedAt = DateTime.UtcNow;

                await _userWalletRepository.UpdateUserWalletAsync(wallet, cancellationToken);

                // Tạo bản ghi giao dịch
                var transaction = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = withdrawalRequest.UserId,
                    Amount = -withdrawalRequest.Amount, // Số âm cho rút tiền
                    Description = $"Withdrawal to {withdrawalRequest.BankName} - {withdrawalRequest.AccountNumber}",
                    TransactionType = "Withdrawal",
                    CreatedAt = DateTime.UtcNow,
                    ReferenceId = withdrawalRequest.Id // Sử dụng trực tiếp Guid thay vì string
                };

                await _walletTransactionRepository.AddWalletTransactionAsync(transaction, cancellationToken);

                // Tạo event rút tiền hoàn thành (sử dụng class IntegrationEvent để đồng bộ với hệ thống)
                var withdrawalEvent = new WithdrawalCompletedEvent(
                    transaction.Id,
                    withdrawalRequest.UserId,
                    withdrawalRequest.Amount,
                    DateTime.UtcNow,
                    $"Withdrawal to {withdrawalRequest.BankName} - {withdrawalRequest.AccountNumber}"
                );

                await _outboxService.SaveMessageAsync(withdrawalEvent);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }

    // Event class dựa trên PaymentBaseEvent
    public record WithdrawalCompletedEvent(
        Guid TransactionId,
        Guid UserId,
        decimal Amount,
        DateTime Timestamp,
        string Description) : PaymentBaseEvent(TransactionId, UserId, Amount, Timestamp, Description);
}