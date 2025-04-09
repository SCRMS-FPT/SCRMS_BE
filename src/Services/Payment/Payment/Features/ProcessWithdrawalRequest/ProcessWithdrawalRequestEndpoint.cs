using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Payment.API.Features.ProcessWithdrawalRequest
{
    public class ProcessWithdrawalRequestEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/admin/payments/withdrawal-requests/{requestId}", async (
                ISender sender,
                HttpContext httpContext,
                Guid requestId,
                [FromBody] ProcessWithdrawalRequestDto request) =>
            {
                var adminUserId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());

                var command = new ProcessWithdrawalRequestCommand(
                    requestId,
                    request.Status,
                    request.AdminNote,
                    adminUserId);

                var result = await sender.Send(command);
                return Results.Ok(new { Success = result });
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("ProcessWithdrawalRequest");
        }
    }

    public class ProcessWithdrawalRequestDto
    {
        [Required]
        [RegularExpression("^(Approved|Rejected)$")]
        public string Status { get; set; }

        public string AdminNote { get; set; }
    }
}