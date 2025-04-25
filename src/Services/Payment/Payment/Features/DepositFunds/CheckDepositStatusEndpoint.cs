using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Payment.API.Features.DepositFunds
{
    public class CheckDepositStatusEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/payments/wallet/deposit/{id}/status", async (
                [FromRoute] Guid id,
                ISender sender) =>
            {
                var result = await sender.Send(new CheckDepositStatusQuery(id));
                if (result == null)
                {
                    return Results.NotFound(new { success = false, message = "Deposit not found" });
                }

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("CheckDepositStatus");
        }
    }
}