using System.Security.Claims;

namespace Payment.API.Features.ProcessBookingPayment
{
    public class ProcessBookingPaymentEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/payments/wallet/booking", async (ProcessBookingPaymentRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var command = new ProcessBookingPaymentCommand(userId, request.Amount, request.Description);
                var transactionId = await sender.Send(command);
                return Results.Created($"/api/payments/wallet/transactions/{transactionId}", new { Id = transactionId });
            })
            .RequireAuthorization()
            .WithName("ProcessBookingPayment");
        }
    }

    public record ProcessBookingPaymentRequest(decimal Amount, string Description);
}