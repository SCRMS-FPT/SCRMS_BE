using MassTransit;
using Payment.API.Data.Repositories;
using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.Outbox;
using Payment.API.Data;

namespace Payment.API.Features.DepositFunds
{
    public record DepositFundsCommand(
        Guid UserId,
        decimal Amount,
        Guid? TransactionReference,
        string PaymentType,
        string Description,
        string PackageType = null,
        Guid? CoachId = null,
        Guid? BookingId = null,
        Guid? PackageId = null) : IRequest<Guid>;

    public class DepositFundsHandler : IRequestHandler<DepositFundsCommand, Guid>
    {
        private readonly IUserWalletRepository _userWalletRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly PaymentDbContext _context;
        private readonly IOutboxService _outboxService;
        private readonly IUnitOfWork _unitOfWork;

        public DepositFundsHandler(
            IUserWalletRepository userWalletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            PaymentDbContext context,
            IOutboxService outboxService,
            IUnitOfWork unitOfWork)
        {
            _userWalletRepository = userWalletRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _context = context;
            _outboxService = outboxService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(DepositFundsCommand request, CancellationToken cancellationToken)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be positive.");

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var wallet = await _userWalletRepository.GetUserWalletByUserIdAsync(request.UserId, cancellationToken);
                if (wallet == null)
                {
                    wallet = new UserWallet { UserId = request.UserId, Balance = 0, UpdatedAt = DateTime.UtcNow };
                    await _userWalletRepository.AddUserWalletAsync(wallet, cancellationToken);
                }

                var transactionRecord = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    TransactionType = "deposit",
                    ReferenceId = request.TransactionReference,
                    Amount = request.Amount,
                    Description = $"Deposit via payment gateway, Ref: {request.TransactionReference}",
                    CreatedAt = DateTime.UtcNow
                };

                wallet.Balance += request.Amount;
                wallet.UpdatedAt = DateTime.UtcNow;

                await _walletTransactionRepository.AddWalletTransactionAsync(transactionRecord, cancellationToken);
                await _userWalletRepository.UpdateUserWalletAsync(wallet, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // Sau khi xử lý thành công, xác định loại thanh toán
                if (request.PaymentType == "ServicePackage" || request.PaymentType.StartsWith("Identity"))
                {
                    // Tạo event nâng cấp tài khoản
                    var servicePackageEvent = new ServicePackagePaymentEvent(
                        transactionRecord.Id,
                        request.UserId,
                        transactionRecord.ReferenceId,
                        request.Amount,
                        DateTime.UtcNow,
                        request.Description ?? "Package payment",
                        request.PackageType,
                        DateTime.UtcNow.AddMonths(1) // thời hạn gói
                    );

                    // Lưu vào outbox
                    await _outboxService.SaveMessageAsync(servicePackageEvent);
                }
                else if (request.PaymentType == "CoachBooking" || request.PaymentType.StartsWith("Coach"))
                {
                    // Tạo event thanh toán cho coach
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

                    // Lưu vào outbox
                    await _outboxService.SaveMessageAsync(coachEvent);
                }
                else
                {
                    // Legacy event cho khả năng tương thích ngược
                    var paymentEvent = new PaymentSucceededEvent(
                        transactionRecord.Id,
                        request.UserId,
                        request.TransactionReference,
                        request.Amount,
                        DateTime.UtcNow,
                        request.Description,
                        request.PaymentType
                    );

                    // Lưu vào outbox
                    await _outboxService.SaveMessageAsync(paymentEvent);
                }

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