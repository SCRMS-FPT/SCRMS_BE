using System.Security.Claims;

namespace Payment.API.Features.GetWalletBalance
{
    public class GetWalletBalanceEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/payments/wallet", async (ISender sender, HttpContext httpContext) =>
            {
                var userId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var query = new GetWalletBalanceQuery(userId);
                var wallet = await sender.Send(query);
                return Results.Ok(wallet);
            })
            .RequireAuthorization()
            .WithName("GetWalletBalance");
        }
    }
}