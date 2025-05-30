﻿using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BuildingBlocks.Messaging.Outbox;

namespace Payment.API.Features.ProcessBookingPayment
{
    public class ProcessBookingPaymentEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/debug/process-outbox", async (IOutboxService outboxService) =>
            {
                await outboxService.ProcessOutboxMessagesAsync(CancellationToken.None);
                return Results.Ok("Outbox processing triggered");
            });
            app.MapPost("/api/payments/wallet/booking", async (ProcessPaymentRequest request, ISender sender, HttpContext httpContext) =>
            {
                // Get authenticated user ID (this will be the court owner for CourtBookingCash)
                var authUserId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    throw new UnauthorizedAccessException());

                // Xác định ProviderId nếu chưa có
                Guid? providerId = request.ProviderId;
                if (providerId == null && (request.PaymentType == "CoachBooking" || request.PaymentType.StartsWith("Coach")))
                {
                    providerId = request.CoachId;  // Sử dụng CoachId làm ProviderId cho thanh toán Coach
                }

                var command = new ProcessBookingPaymentCommand(
                    authUserId, // Always use authenticated user's ID
                    request.Amount,
                    request.Description,
                    request.PaymentType,
                    request.ReferenceId,
                    request.CoachId,
                    providerId,
                    request.BookingId,
                    request.PackageId,
                    request.Status,
                    request.CustomerId); // Pass customer ID for reference

                var transactionId = await sender.Send(command);
                return Results.Created($"/api/payments/wallet/transactions/{transactionId}",
                    new
                    {
                        Id = transactionId,
                        Amount = request.Amount,
                        PaymentType = request.PaymentType,
                        Timestamp = DateTime.UtcNow
                    });
            })
            .RequireAuthorization()
            .WithName("ProcessBookingPayment");
        }
    }

    public record ProcessPaymentRequest(
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue)]
        decimal Amount,

        [Required(ErrorMessage = "Description is required")]
        string Description,

        [Required(ErrorMessage = "Payment type is required")]
        string PaymentType,

        Guid? ReferenceId = null,
        Guid? CoachId = null,
        Guid? ProviderId = null,
        Guid? BookingId = null,
        Guid? PackageId = null,
        string? Status = "Confirmed",
        Guid? CustomerId = null  // Add CustomerId parameter
    );
}