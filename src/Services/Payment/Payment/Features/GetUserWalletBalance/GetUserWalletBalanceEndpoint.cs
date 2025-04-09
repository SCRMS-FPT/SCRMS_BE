using System.Security.Claims;
using Payment.API.Features.GetWalletBalance;

namespace Payment.API.Features.GetUserWalletBalance
{
    public class GetUserWalletBalanceEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/admin/payments/wallet/{userId}", async (ISender sender, Guid userId) =>
            {
                var query = new GetWalletBalanceQuery(userId);
                var wallet = await sender.Send(query);
                return Results.Ok(wallet);
            })
            .RequireAuthorization("Admin")
            .WithName("GetUserWalletBalance");
        }
    }
}