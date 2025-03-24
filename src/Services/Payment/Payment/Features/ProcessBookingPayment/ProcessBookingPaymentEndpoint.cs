using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BuildingBlocks.Messaging.Outbox;

namespace Payment.API.Features.ProcessBookingPayment
{
    public class ProcessBookingPaymentEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/debug/process-outbox", async (IOutboxService outboxService) =>
            {
                await outboxService.ProcessOutboxMessagesAsync(CancellationToken.None);
                return Results.Ok("Outbox processing triggered");
            });
            app.MapPost("/api/payments/wallet/booking", async (ProcessPaymentRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var command = new ProcessBookingPaymentCommand(
                    userId,
                    request.Amount,
                    request.Description,
                    request.PaymentType,
                    request.ReferenceId,
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
        Guid? CoachId = null,
        Guid? BookingId = null,
        Guid? PackageId = null,
        string? status = "Confirmed"
    );
}