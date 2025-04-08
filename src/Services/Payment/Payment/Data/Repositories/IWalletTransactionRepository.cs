namespace Payment.API.Data.Repositories
{
    public interface IWalletTransactionRepository
    {
        Task AddWalletTransactionAsync(WalletTransaction transaction, CancellationToken cancellationToken);
        Task<long> GetTransactionCountByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<WalletTransaction?> GetByReferenceCodeAsync(Guid referenceCode);
        Task<WalletTransaction> GetRecentTransactionByUserIdAsync(Guid userId, DateTime sinceTime, CancellationToken cancellationToken = default);
        Task<List<WalletTransaction>> GetTransactionsByUserIdAsync(Guid userId, int page, int limit, CancellationToken cancellationToken);
    }
}