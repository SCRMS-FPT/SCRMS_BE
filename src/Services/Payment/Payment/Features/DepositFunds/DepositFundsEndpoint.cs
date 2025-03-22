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
                var command = new DepositFundsCommand(
                    userId,
                    request.Amount,
                    request.Description ?? "Deposit funds");

                var transactionId = await sender.Send(command);

                // Get updated balance
                var wallet = await sender.Send(new GetUserWalletQuery(userId));

                return Results.Created($"/api/payments/wallet/transactions/{transactionId}",
                    new
                    {
                        Id = transactionId,
                        Balance = wallet?.Balance ?? 0,
                        Amount = request.Amount,
                        Timestamp = DateTime.UtcNow
                    });
            })
            .RequireAuthorization()
            .WithName("DepositFunds");
        }
    }

    public record DepositFundsRequest(
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue)]
        decimal Amount,

        string Description
    );

    // Define GetUserWalletQuery if not already defined elsewhere
    public record GetUserWalletQuery(Guid UserId) : IRequest<UserWallet>;
}