using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payment.API.Data.Repositories;
using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.Outbox;
using Payment.API.Data;

namespace Payment.API.Features.ProcessBookingPayment
{
    public record ProcessBookingPaymentCommand(
        Guid UserId,
        decimal Amount,
        string Description,
        string PaymentType,
        Guid? ReferenceId = null,
        Guid? CoachId = null,
        Guid? BookingId = null,
        Guid? PackageId = null,
        string? Status = "Confirmed") : IRequest<Guid>;

    public class ProcessBookingPaymentHandler : IRequestHandler<ProcessBookingPaymentCommand, Guid>
    {
        private readonly IUserWalletRepository _userWalletRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IOutboxService _outboxService;
        private readonly PaymentDbContext _context;

        public ProcessBookingPaymentHandler(
            IUserWalletRepository userWalletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            IOutboxService outboxService,
            PaymentDbContext context)
        {
            _userWalletRepository = userWalletRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _outboxService = outboxService;
            _context = context;
        }

        public async Task<Guid> Handle(ProcessBookingPaymentCommand request, CancellationToken cancellationToken)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be positive.");

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var wallet = await _userWalletRepository.GetUserWalletByUserIdAsync(request.UserId, cancellationToken);

                // Improved error handling for insufficient balance
                if (wallet == null)
                {
                    await HandlePaymentFailure(request, "User wallet not found", cancellationToken);
                    throw new Exception("User wallet not found.");
                }

                if (wallet.Balance < request.Amount)
                {
                    await HandlePaymentFailure(request, "Insufficient balance", cancellationToken);
                    throw new Exception("Insufficient balance.");
                }

                // Continue with existing success case...
                var transactionRecord = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    TransactionType = request.PaymentType,
                    ReferenceId = request.ReferenceId,
                    Amount = -request.Amount,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow
                };

                wallet.Balance -= request.Amount;
                wallet.UpdatedAt = DateTime.UtcNow;

                await _walletTransactionRepository.AddWalletTransactionAsync(transactionRecord, cancellationToken);
                await _userWalletRepository.UpdateUserWalletAsync(wallet, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                // Generate the appropriate event based on payment type
                if (request.PaymentType == "ServicePackage" || request.PaymentType.StartsWith("Identity"))
                {
                    // Create service package upgrade event
                    var servicePackageEvent = new ServicePackagePaymentEvent(
                        transactionRecord.Id,
                        request.UserId,
                        request.ReferenceId,
                        request.Amount,
                        DateTime.UtcNow,
                        request.Description
                    );

                    await _outboxService.SaveMessageAsync(servicePackageEvent);
                }
                else if (request.PaymentType == "CoachBooking" || request.PaymentType.StartsWith("Coach"))
                {
                    // Create coach payment event
                    var coachEvent = new CoachPaymentEvent(
                        transactionRecord.Id,
                        request.UserId,
                        request.CoachId.Value,
                        request.Amount,
                        DateTime.UtcNow,
                        request.Description,
                        request.BookingId,
                        request.PackageId
                    );

                    await _outboxService.SaveMessageAsync(coachEvent);
                }
                else if (request.PaymentType == "CourtBooking" || request.PaymentType.StartsWith("Court"))
                {
                    // Create a generic payment event for court booking
                    var paymentEvent = new BookCourtSucceededEvent(
                        transactionRecord.Id,
                        request.UserId,
                        request.ReferenceId,
                        request.Amount,
                        DateTime.UtcNow,
                        request.Description,
                        "CourtBooking", request.Status
                    );

                    await _outboxService.SaveMessageAsync(paymentEvent);
                }
                else
                {
                    // Generic payment event for other types
                    var paymentEvent = new PaymentSucceededEvent(
                        transactionRecord.Id,
                        request.UserId,
                        request.ReferenceId,
                        request.Amount,
                        DateTime.UtcNow,
                        request.Description,
                        request.PaymentType
                    );

                    await _outboxService.SaveMessageAsync(paymentEvent);
                }

                await transaction.CommitAsync(cancellationToken);
                return transactionRecord.Id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                // Handle any other unexpected errors
                await HandlePaymentFailure(request, ex.Message, cancellationToken);
                throw;
            }
        }

        // New method to handle payment failures
        private async Task HandlePaymentFailure(ProcessBookingPaymentCommand request, string errorMessage, CancellationToken cancellationToken)
        {
            // Create a payment failure event
            var failureEvent = new PaymentFailedEvent(
                Guid.NewGuid(),  // No transaction ID since payment failed
                request.UserId,
                request.BookingId,
                request.Amount,
                DateTime.UtcNow,
                $"Payment failed: {errorMessage}",
                request.PaymentType,
                "Failed"
            );

            // Save to outbox for reliability
            await _outboxService.SaveMessageAsync(failureEvent);
        }
    }

}