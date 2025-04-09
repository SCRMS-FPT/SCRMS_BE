using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payment.API.Data.Repositories;
using Payment.API.Data;
using Payment.API.Data.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Payment.API.Features.CreateWithdrawalRequest
{
    public record CreateWithdrawalRequestCommand(
        Guid UserId,
        decimal Amount,
        string BankName,
        string AccountNumber,
        string AccountHolderName) : IRequest<Guid>;

    public class CreateWithdrawalRequestHandler : IRequestHandler<CreateWithdrawalRequestCommand, Guid>
    {
        private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
        private readonly IUserWalletRepository _userWalletRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateWithdrawalRequestHandler(
            IWithdrawalRequestRepository withdrawalRequestRepository,
            IUserWalletRepository userWalletRepository,
            IUnitOfWork unitOfWork)
        {
            _withdrawalRequestRepository = withdrawalRequestRepository;
            _userWalletRepository = userWalletRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateWithdrawalRequestCommand request, CancellationToken cancellationToken)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Withdrawal amount must be greater than zero");

            // Kiểm tra số dư
            var wallet = await _userWalletRepository.GetUserWalletByUserIdAsync(request.UserId, cancellationToken);
            if (wallet == null || wallet.Balance < request.Amount)
                throw new InvalidOperationException("Insufficient funds in wallet");

            // Tạo yêu cầu rút tiền
            var withdrawalRequest = new WithdrawalRequest
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Amount = request.Amount,
                BankName = request.BankName,
                AccountNumber = request.AccountNumber,
                AccountHolderName = request.AccountHolderName,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _withdrawalRequestRepository.AddAsync(withdrawalRequest, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return withdrawalRequest.Id;
        }
    }
}