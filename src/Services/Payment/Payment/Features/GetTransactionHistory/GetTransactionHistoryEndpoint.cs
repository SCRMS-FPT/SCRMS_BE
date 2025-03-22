using System.Security.Claims;
using BuildingBlocks.Pagination;

namespace Payment.API.Features.GetTransactionHistory
{
    public class GetTransactionHistoryEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/payments/wallet/transactions", async (ISender sender, HttpContext httpContext, int page = 1, int limit = 10) =>
            {
                var userId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var query = new GetTransactionHistoryQuery(userId, page, limit);
                var transactions = await sender.Send(query);
                return Results.Ok(transactions);
            })
            .RequireAuthorization()
            .WithName("GetTransactionHistory");
        }
    }
}