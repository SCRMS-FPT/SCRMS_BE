﻿using Microsoft.EntityFrameworkCore;
using Payment.API.Data.Repositories;

namespace Payment.API.Features.ProcessBookingPayment
{
    public record ProcessBookingPaymentCommand(Guid UserId, decimal Amount, string Description) : IRequest<Guid>;

    public class ProcessBookingPaymentHandler : IRequestHandler<ProcessBookingPaymentCommand, Guid>
    {
        private readonly IUserWalletRepository _userWalletRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly PaymentDbContext _context;

        public ProcessBookingPaymentHandler(
            IUserWalletRepository userWalletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            PaymentDbContext context)
        {
            _userWalletRepository = userWalletRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _context = context;
        }

        public async Task<Guid> Handle(ProcessBookingPaymentCommand request, CancellationToken cancellationToken)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be positive.");

            var wallet = await _userWalletRepository.GetUserWalletByUserIdAsync(request.UserId, cancellationToken);
            if (wallet == null || wallet.Balance < request.Amount)
                throw new Exception("Insufficient balance.");

            var transactionRecord = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                TransactionType = "booking",
                Amount = -request.Amount,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            wallet.Balance -= request.Amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _walletTransactionRepository.AddWalletTransactionAsync(transactionRecord, cancellationToken);
            await _userWalletRepository.UpdateUserWalletAsync(wallet, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return transactionRecord.Id;
        }
    }
}