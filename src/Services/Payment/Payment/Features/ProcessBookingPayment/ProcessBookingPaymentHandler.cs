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
        Guid? ProviderId = null,
        Guid? BookingId = null,
        Guid? PackageId = null,
        string? Status = "Confirmed",
        Guid? CustomerId = null) : IRequest<Guid>;  // Add CustomerId parameter

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
                throw new ArgumentException("Số tiền phải là số dương.");

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Special handling for cash payment at court (marked as completed by court owner)
                if (request.PaymentType == "CourtBookingCash")
                {
                    // Create a transaction record without modifying any wallets
                    // UserId is the court owner's ID (authenticated user)
                    var cashTransaction = new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = request.UserId, // Court owner ID (from JWT)
                        TransactionType = "CourtBookingRevenue",
                        ReferenceId = request.ReferenceId ?? request.BookingId,
                        Amount = request.Amount,
                        Description = request.Description ??
                            (request.CustomerId != null
                                ? $"Thanh toán tiền mặt tại sân cho khách hàng {request.CustomerId}"
                                : "Thanh toán tiền mặt tại sân"),
                        CreatedAt = DateTime.UtcNow
                    };

                    await _walletTransactionRepository.AddWalletTransactionAsync(cashTransaction, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return cashTransaction.Id;
                }

                var wallet = await _userWalletRepository.GetUserWalletByUserIdAsync(request.UserId, cancellationToken);

                if (wallet == null)
                {
                    await HandlePaymentFailure(request, "Không tìm thấy ví người dùng", cancellationToken);
                    throw new Exception("Không tìm thấy ví người dùng.");
                }

                if (wallet.Balance < request.Amount)
                {
                    await HandlePaymentFailure(request, "Số dư không đủ", cancellationToken);
                    throw new Exception("Số dư không đủ.");
                }

                // 1. TRỪ TIỀN NGƯỜI DÙNG
                var userTransaction = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    TransactionType = request.PaymentType,
                    ReferenceId = request.ReferenceId ?? request.BookingId,
                    Amount = -request.Amount,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow
                };

                wallet.Balance -= request.Amount;
                wallet.UpdatedAt = DateTime.UtcNow;

                await _walletTransactionRepository.AddWalletTransactionAsync(userTransaction, cancellationToken);
                await _userWalletRepository.UpdateUserWalletAsync(wallet, cancellationToken);

                // 2. CỘNG TIỀN CHO PROVIDER (NẾU CÓ)
                if (request.ProviderId.HasValue && request.ProviderId.Value != Guid.Empty)
                {
                    // Tìm hoặc tạo ví của provider
                    var providerWallet = await _userWalletRepository.GetUserWalletByUserIdAsync(request.ProviderId.Value, cancellationToken);

                    if (providerWallet == null)
                    {
                        // Tạo ví mới nếu chưa có
                        providerWallet = new UserWallet
                        {
                            UserId = request.ProviderId.Value,
                            Balance = 0,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _userWalletRepository.AddUserWalletAsync(providerWallet, cancellationToken);
                    }

                    // Cộng toàn bộ số tiền vào ví provider (không trừ hoa hồng)
                    providerWallet.Balance += request.Amount;
                    providerWallet.UpdatedAt = DateTime.UtcNow;
                    await _userWalletRepository.UpdateUserWalletAsync(providerWallet, cancellationToken);

                    // Tạo giao dịch cộng tiền
                    var providerTransaction = new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = request.ProviderId.Value,
                        TransactionType = $"{request.PaymentType}Revenue",
                        ReferenceId = request.ReferenceId ?? request.BookingId,
                        Amount = request.Amount, // Cộng toàn bộ số tiền (không trừ phí)
                        Description = $"Doanh thu từ {request.Description}",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _walletTransactionRepository.AddWalletTransactionAsync(providerTransaction, cancellationToken);
                }

                // 3. TẠO EVENT PHÙ HỢP THEO LOẠI THANH TOÁN
                // Các đoạn code còn lại không thay đổi
                if (request.PaymentType == "ServicePackage" || request.PaymentType.StartsWith("Identity"))
                {
                    // Code xử lý ServicePackage
                    var servicePackageEvent = new ServicePackagePaymentEvent(
                        userTransaction.Id,
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
                        userTransaction.Id,
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
                else if (request.PaymentType == "CourtBooking" || request.PaymentType == "CourtBookingAdditional" || request.PaymentType.StartsWith("Court"))
                {
                    var paymentEvent = new BookCourtSucceededEvent(
                        userTransaction.Id,
                        request.UserId,
                        request.ReferenceId ?? request.BookingId, // Use BookingId as ReferenceId if provided
                        request.Amount,
                        DateTime.UtcNow,
                        request.Description,
                        request.PaymentType, // Pass the exact payment type (CourtBooking or CourtBookingAdditional)
                        request.Status
                    );

                    await _outboxService.SaveMessageAsync(paymentEvent);
                }
                else
                {
                    // Generic payment event for other types
                    var paymentEvent = new PaymentSucceededEvent(
                        userTransaction.Id,
                        request.UserId,
                        request.ReferenceId,
                        request.Amount,
                        DateTime.UtcNow,
                        request.Description,
                        request.PaymentType
                    );

                    await _outboxService.SaveMessageAsync(paymentEvent);
                }

                try
                {
                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Detach the original entity from context tracking
                    var originalEntry = _context.ChangeTracker.Entries<UserWallet>()
                        .FirstOrDefault(e => e.Entity.UserId == request.UserId);
                    if (originalEntry != null)
                    {
                        originalEntry.State = EntityState.Detached;
                    }

                    // Now reload the entity
                    var freshWallet = await _userWalletRepository.GetUserWalletByUserIdAsync(request.UserId, cancellationToken);
                    if (freshWallet == null) throw new Exception("Wallet no longer exists");

                    freshWallet.Balance -= request.Amount;
                    freshWallet.UpdatedAt = DateTime.UtcNow;

                    await _userWalletRepository.UpdateUserWalletAsync(freshWallet, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                await transaction.CommitAsync(cancellationToken);
                return userTransaction.Id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
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