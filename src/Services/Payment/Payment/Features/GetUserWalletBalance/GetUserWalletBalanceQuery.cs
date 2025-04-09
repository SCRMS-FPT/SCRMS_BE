using System;
using MediatR;
using Payment.API.Data.Models;

namespace Payment.API.Features.GetUserWalletBalance
{
    public record GetUserWalletBalanceQuery(Guid UserId) : IRequest<UserWallet>;
}