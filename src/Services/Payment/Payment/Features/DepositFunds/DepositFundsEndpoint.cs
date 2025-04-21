using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Payment.API.Features.DepositFunds
{
    public class DepositFundsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/payments/wallet/deposit", async (DepositFundsRequest request, ISender sender, HttpContext httpContext, IConfiguration configuration) =>
            {
                var userId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var command = new DepositFundsCommand(
                    userId,
                    request.Amount,
                    request.Description ?? "Deposit funds");

                var result = await sender.Send(command);

                // Get bank account information from configuration
                var bankAccount = configuration["Sepay:BankAccount"];
                var bankName = configuration["Sepay:BankName"];

                // Generate QR code URL (using SePay's QR code service)
                var qrCodeUrl = $"https://qr.sepay.vn/img?acc={bankAccount}&bank={bankName}&amount={request.Amount}&des={result.DepositCode}";

                return Results.Ok(new
                {
                    Id = result.DepositId,
                    Code = result.DepositCode,
                    Amount = result.Amount,
                    Instructions = "Please transfer the exact amount to our bank account and include this code in your transfer description. Your account will be credited once we receive the payment.",
                    BankInfo = result.BankInfo,
                    QrCodeUrl = qrCodeUrl,
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