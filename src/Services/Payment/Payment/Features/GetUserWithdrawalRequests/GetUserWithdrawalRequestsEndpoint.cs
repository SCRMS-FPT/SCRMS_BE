using System.Security.Claims;

namespace Payment.API.Features.GetUserWithdrawalRequests
{
    public class GetUserWithdrawalRequestsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/payments/wallet/withdrawals", async (ISender sender, HttpContext httpContext) =>
            {
                var userId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var query = new GetUserWithdrawalRequestsQuery(userId);
                var requests = await sender.Send(query);
                return Results.Ok(requests);
            })
            .RequireAuthorization()
            .WithName("GetUserWithdrawalRequests");
        }
    }
}