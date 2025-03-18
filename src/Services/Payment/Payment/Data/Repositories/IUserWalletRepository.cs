namespace Payment.API.Data.Repositories
{
    public interface IUserWalletRepository
    {
        Task<UserWallet?> GetUserWalletByUserIdAsync(Guid userId, CancellationToken cancellationToken);

        Task AddUserWalletAsync(UserWallet wallet, CancellationToken cancellationToken);

        Task UpdateUserWalletAsync(UserWallet wallet, CancellationToken cancellationToken);
    }
}