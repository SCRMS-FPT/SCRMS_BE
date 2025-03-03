using System.ComponentModel.DataAnnotations;
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

    public record DepositFundsRequest(
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be ≥ 0.01")]
    decimal Amount,

    [Required(ErrorMessage = "Transaction reference required")]
    [StringLength(100, MinimumLength = 5)]
    string TransactionReference
);
}