using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Payment.API.Features.CreateWithdrawalRequest
{
    public class CreateWithdrawalRequestEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/payments/wallet/withdraw", async (
                ISender sender,
                HttpContext httpContext,
                [FromBody] WithdrawalRequestDto request) =>
            {
                var userId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());

                var command = new CreateWithdrawalRequestCommand(
                    userId,
                    request.Amount,
                    request.BankName,
                    request.AccountNumber,
                    request.AccountHolderName);

                var requestId = await sender.Send(command);
                return Results.Ok(new { RequestId = requestId });
            })
            .RequireAuthorization()
            .WithName("CreateWithdrawalRequest");
        }
    }

    public class WithdrawalRequestDto
    {
        [Required]
        [Range(10000, 100000000)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(100)]
        public string BankName { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string AccountHolderName { get; set; }
    }
}