using Carter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using System.Threading.Tasks;

namespace Payment.API.Features.SePay
{
    public class SepayWebhookEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/payments/sepay-webhook", async (SepayWebhookRequest request, ISender sender) =>
            {
                var result = await sender.Send(new ProcessSepayWebhookCommand(request));
                return result ? Results.Ok(new { success = true }) : Results.BadRequest(new { success = false });
            })
            .WithName("SepayWebhook");
        }
    }

    public record SepayWebhookRequest(
        string TransferType,  // "in" = tiền vào, "out" = tiền ra
        string AccountNumber,
        decimal TransferAmount,
        string Content,
        string ReferenceCode
    );
}

