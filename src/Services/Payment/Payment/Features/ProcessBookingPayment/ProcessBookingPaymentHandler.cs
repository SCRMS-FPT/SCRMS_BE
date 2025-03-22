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
        string PackageType = null,
        DateTime? ValidUntil = null,
        Guid? CoachId = null,
        Guid? BookingId = null,
        Guid? PackageId = null) : IRequest<Guid>;

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
                if (wallet == null || wallet.Balance < request.Amount)
                    throw new Exception("Insufficient balance.");

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
                        request.Description,
                        request.PackageType,
                        request.ValidUntil ?? DateTime.UtcNow.AddMonths(1) // Default package duration
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
                    var paymentEvent = new PaymentSucceededEvent(
                        transactionRecord.Id,
                        request.UserId,
                        request.ReferenceId,
                        request.Amount,
                        DateTime.UtcNow,
                        request.Description,
                        "CourtBooking"
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
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}