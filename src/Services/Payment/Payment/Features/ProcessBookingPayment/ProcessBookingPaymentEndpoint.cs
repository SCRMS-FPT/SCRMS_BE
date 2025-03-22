using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Payment.API.Features.ProcessBookingPayment
{
    public class ProcessBookingPaymentEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/payments/wallet/booking", async (ProcessPaymentRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var command = new ProcessBookingPaymentCommand(
                    userId,
                    request.Amount,
                    request.Description,
                    request.PaymentType,
                    request.ReferenceId,
                    request.PackageType,
                    request.ValidUntil,
                    request.CoachId,
                    request.BookingId,
                    request.PackageId);

                var transactionId = await sender.Send(command);
                return Results.Created($"/api/payments/wallet/transactions/{transactionId}",
                    new
                    {
                        Id = transactionId,
                        Amount = request.Amount,
                        PaymentType = request.PaymentType,
                        Timestamp = DateTime.UtcNow
                    });
            })
            .RequireAuthorization()
            .WithName("ProcessBookingPayment");
        }
    }

    public record ProcessPaymentRequest(
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue)]
        decimal Amount,

        [Required(ErrorMessage = "Description is required")]
        string Description,

        [Required(ErrorMessage = "Payment type is required")]
        string PaymentType,

        Guid? ReferenceId = null,
        string PackageType = null,
        DateTime? ValidUntil = null,
        Guid? CoachId = null,
        Guid? BookingId = null,
        Guid? PackageId = null
    );
}