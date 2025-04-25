namespace Payment.API.Features.DepositFunds
{
    public class SePayWebhookEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/payments/sepay/webhook", async (
                SePayWebhookModel webhook,
                ISender sender,
                IConfiguration configuration,
                ILogger<SePayWebhookEndpoint> logger,
                HttpContext httpContext) =>
            {
                // Verify the request is from SePay by checking the IP
                //string allowedIp = configuration["Sepay:AllowedIp"];
                //string requestIp = httpContext.Connection.RemoteIpAddress?.ToString();

                //logger.LogInformation($"Received webhook from IP: {requestIp}");

                //if (!string.IsNullOrEmpty(allowedIp) && requestIp != allowedIp)
                //{
                //    logger.LogWarning($"Unauthorized IP: {requestIp}, expected: {allowedIp}");
                //    return Results.Json(new { success = false, message = "Unauthorized IP" }, statusCode: (int)HttpStatusCode.Forbidden);
                //}

                // Process the webhook using the command handler
                var command = new ProcessSePayWebhookCommand(webhook);
                var result = await sender.Send(command);

                if (!result.Success)
                {
                    logger.LogWarning($"Webhook processing failed: {result.Message}");
                    return Results.Ok(new { success = false, message = result.Message });
                }

                logger.LogInformation($"Webhook processed successfully. Transaction ID: {result.TransactionId}");
                return Results.Ok(new { success = true, transactionId = result.TransactionId });
            })
            .WithName("ProcessSePayWebhook")
            .WithDisplayName("Process SePay Webhook");
        }
    }
}