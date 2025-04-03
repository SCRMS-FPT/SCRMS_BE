namespace Payment.API.Data.Repositories
{
    public interface IWalletTransactionRepository
    {
        Task AddWalletTransactionAsync(WalletTransaction transaction, CancellationToken cancellationToken);
        Task<long> GetTransactionCountByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<WalletTransaction?> GetByReferenceCodeAsync(Guid referenceCode);
        Task<List<WalletTransaction>> GetTransactionsByUserIdAsync(Guid userId, int page, int limit, CancellationToken cancellationToken);
    }
}