using Microsoft.EntityFrameworkCore;
using Payment.API.Data.Repositories;

namespace Payment.API.Features.GetWalletBalance
{
    public record GetWalletBalanceQuery(Guid UserId) : IRequest<UserWallet>;

    public class GetWalletBalanceHandler : IRequestHandler<GetWalletBalanceQuery, UserWallet>
    {
        private readonly IUserWalletRepository _userWalletRepository;

        public GetWalletBalanceHandler(IUserWalletRepository userWalletRepository)
        {
            _userWalletRepository = userWalletRepository;
        }

        public async Task<UserWallet> Handle(GetWalletBalanceQuery request, CancellationToken cancellationToken)
        {
            var wallet = await _userWalletRepository.GetUserWalletByUserIdAsync(request.UserId, cancellationToken);
            if (wallet == null)
                return new UserWallet
                {
                    UserId = request.UserId,
                    Balance = 0,
                    UpdatedAt = DateTime.UtcNow
                };

            return wallet;
        }
    }
}