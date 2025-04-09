using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Payment.API.Data.Models;
using Payment.API.Data.Repositories;

namespace Payment.API.Features.GetUserWalletBalance
{
    public class GetUserWalletBalanceHandler : IRequestHandler<GetUserWalletBalanceQuery, UserWallet>
    {
        private readonly IUserWalletRepository _userWalletRepository;

        public GetUserWalletBalanceHandler(IUserWalletRepository userWalletRepository)
        {
            _userWalletRepository = userWalletRepository;
        }

        public async Task<UserWallet> Handle(GetUserWalletBalanceQuery request, CancellationToken cancellationToken)
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