using System.Security.Claims;

namespace Payment.API.Features.DepositFunds
{
    public class DepositFundsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/payments/wallet/deposit", async (DepositFundsRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var command = new DepositFundsCommand(userId, request.Amount, request.TransactionReference);
                var transactionId = await sender.Send(command);
                return Results.Created($"/api/payments/wallet/transactions/{transactionId}", new { Id = transactionId });
            })
            .RequireAuthorization()
            .WithName("DepositFunds");
        }
    }

    public record DepositFundsRequest(decimal Amount, string TransactionReference);
}