using Payment.API.Data.Repositories;
using Payment.API.Features.DepositFunds;

public class CheckPendingTransactionHandler : IRequestHandler<CheckPendingTransactionQuery, TransactionStatusResult>
{
    private readonly IWalletTransactionRepository _transactionRepository;

    public CheckPendingTransactionHandler(IWalletTransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<TransactionStatusResult> Handle(CheckPendingTransactionQuery request, CancellationToken cancellationToken)
    {
        // Tìm giao dịch gần nhất của người dùng trong vòng 15 phút qua
        var recentTime = DateTime.UtcNow.AddMinutes(-15);
        var transaction = await _transactionRepository.GetRecentTransactionByUserIdAsync(request.UserId, recentTime, cancellationToken);

        if (transaction == null)
            return new TransactionStatusResult(false, null, 0);

        return new TransactionStatusResult(
            Completed: true,
            TransactionId: transaction.Id.ToString(),
            Amount: transaction.Amount
        );
    }
}