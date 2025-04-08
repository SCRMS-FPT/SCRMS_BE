// Thêm endpoint mới để kiểm tra giao dịch đang chờ
using System.Security.Claims;

namespace Payment.API.Features.DepositFunds
{
    public class CheckPendingTransactionEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/payments/pending-transaction", async (ISender sender, HttpContext httpContext) =>
            {
                var userId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());

                var query = new CheckPendingTransactionQuery(userId);
                var result = await sender.Send(query);

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("CheckPendingTransaction");
        }
    }

    public record CheckPendingTransactionQuery(Guid UserId) : IRequest<TransactionStatusResult>;
    public record TransactionStatusResult(bool Completed, string TransactionId, decimal Amount);
}