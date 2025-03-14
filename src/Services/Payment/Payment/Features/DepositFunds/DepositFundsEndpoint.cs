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
                    request.TransactionReference,
                    request.PaymentType,
                    request.Description ?? "Deposit funds",
                    request.PackageType,
                    request.CoachId,
                    request.BookingId,
                    request.PackageId);
                var transactionId = await sender.Send(command);
                return Results.Created($"/api/payments/wallet/transactions/{transactionId}", new { Id = transactionId });
            })
            .RequireAuthorization()
            .WithName("DepositFunds");
        }
    }

    public record DepositFundsRequest(
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue)]
        decimal Amount,

        [StringLength(100, MinimumLength = 5)]
        Guid? TransactionReference,

        [StringLength(100, MinimumLength = 5)]
        string PaymentType,

        [StringLength(100, MinimumLength = 5)]
        string Description,

        [StringLength(100, MinimumLength = 5)]
        string PackageType,

        Guid? CoachId,

        Guid? BookingId,

        Guid? PackageId
    );
}