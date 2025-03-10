namespace Payment.API.Data.Repositories
{
    public interface IWalletTransactionRepository
    {
        Task AddWalletTransactionAsync(WalletTransaction transaction, CancellationToken cancellationToken);

        Task<List<WalletTransaction>> GetTransactionsByUserIdAsync(Guid userId, int page, int limit, CancellationToken cancellationToken);
    }
}