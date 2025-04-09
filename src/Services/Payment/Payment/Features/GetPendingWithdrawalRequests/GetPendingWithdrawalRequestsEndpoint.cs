namespace Payment.API.Features.GetPendingWithdrawalRequests
{
    public class GetPendingWithdrawalRequestsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/admin/payments/withdrawal-requests", async (ISender sender, int page = 1, int limit = 10) =>
            {
                var query = new GetPendingWithdrawalRequestsQuery(page, limit);
                var requests = await sender.Send(query);
                return Results.Ok(requests);
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("GetPendingWithdrawalRequests");
        }
    }
}