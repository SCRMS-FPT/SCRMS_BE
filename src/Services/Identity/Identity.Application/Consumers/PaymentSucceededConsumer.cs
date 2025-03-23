using BuildingBlocks.Messaging.Events;
using Identity.Application.Data.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Consumers
{
    public class PaymentSucceededConsumer : IConsumer<PaymentSucceededEvent>, IConsumer<ServicePackagePaymentEvent>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IServicePackageRepository _packageRepository;
        private readonly UserManager<User> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly ILogger<PaymentSucceededConsumer> _logger;

        public PaymentSucceededConsumer(
            ISubscriptionRepository subscriptionRepository,
            IServicePackageRepository packageRepository,
            UserManager<User> userManager,
            IApplicationDbContext context,
            ILogger<PaymentSucceededConsumer> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _packageRepository = packageRepository;
            _userManager = userManager;
            _context = context;
            _logger = logger;

            _logger.LogInformation("🔧 PaymentSucceededConsumer initialized");
        }

        public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
        {
            var paymentEvent = context.Message;

            _logger.LogInformation("📥 RECEIVED PaymentSucceededEvent: TransactionId={TransactionId}, UserId={UserId}, Amount={Amount}, PaymentType={PaymentType}",
                paymentEvent.TransactionId, paymentEvent.UserId, paymentEvent.Amount, paymentEvent.PaymentType);
            _logger.LogDebug("📋 Message details: MessageId={MessageId}, SourceAddress={SourceAddress}, SentTime={SentTime}",
                context.MessageId, context.SourceAddress, context.SentTime);

            // Chỉ xử lý nếu loại thanh toán liên quan đến Identity service
            if (paymentEvent.PaymentType == "ServicePackage" ||
                paymentEvent.PaymentType == "AccountUpgrade" ||
                paymentEvent.PaymentType.StartsWith("Identity"))
            {
                _logger.LogInformation("✅ Message is relevant for Identity service: PaymentType={PaymentType}",
                    paymentEvent.PaymentType);

                try
                {
                    // Thực hiện xử lý nâng cấp tài khoản
                    await ProcessServicePackagePayment(paymentEvent);
                    _logger.LogInformation("✅ Successfully processed PaymentSucceededEvent: TransactionId={TransactionId}",
                        paymentEvent.TransactionId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ ERROR processing PaymentSucceededEvent: TransactionId={TransactionId}",
                        paymentEvent.TransactionId);
                    throw; // Rethrow to let MassTransit handle it
                }
            }
            else
            {
                _logger.LogInformation("⏭️ Skipping payment event not related to Identity service: PaymentType={PaymentType}",
                    paymentEvent.PaymentType);
            }
        }

        // Xử lý event chuyên biệt cho gói dịch vụ
        public async Task Consume(ConsumeContext<ServicePackagePaymentEvent> context)
        {
            var paymentEvent = context.Message;

            _logger.LogInformation("📥 RECEIVED ServicePackagePaymentEvent: TransactionId={TransactionId}, UserId={UserId}, Amount={Amount}, ServicePackageId={ServicePackageId}",
                paymentEvent.TransactionId, paymentEvent.UserId, paymentEvent.Amount, paymentEvent.ServicePackageId);
            _logger.LogDebug("📋 Message details: MessageId={MessageId}, SourceAddress={SourceAddress}, SentTime={SentTime}",
                context.MessageId, context.SourceAddress, context.SentTime);
            _logger.LogDebug("📦 Event full data: {@EventData}", paymentEvent); // Log the entire event

            try
            {
                // Thực hiện xử lý nâng cấp tài khoản với thông tin chi tiết hơn
                await ProcessServicePackagePaymentDetailed(paymentEvent);
                _logger.LogInformation("✅ Successfully processed ServicePackagePaymentEvent: TransactionId={TransactionId}",
                    paymentEvent.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR processing ServicePackagePaymentEvent: TransactionId={TransactionId}",
                    paymentEvent.TransactionId);
                throw; // Rethrow to let MassTransit handle it
            }
        }

        private async Task ProcessServicePackagePayment(PaymentBaseEvent payment)
        {
            _logger.LogInformation("🔄 Starting ProcessServicePackagePayment for UserId={UserId}, TransactionId={TransactionId}",
                payment.UserId, payment.TransactionId);

            // Xử lý thanh toán cơ bản cho các event cũ
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogDebug("🔄 Database transaction started");

                // Tìm gói dịch vụ dựa vào ReferenceId nếu có
                if (payment is PaymentSucceededEvent paymentSucceeded && paymentSucceeded.ReferenceId.HasValue)
                {
                    _logger.LogInformation("🔍 Looking up service package with ID={PackageId}",
                        paymentSucceeded.ReferenceId.Value);

                    var servicePackage = await _packageRepository.GetServicePackageByIdAsync(paymentSucceeded.ReferenceId.Value);
                    if (servicePackage != null)
                    {
                        _logger.LogInformation("✅ Found service package: ID={PackageId}, Name={PackageName}, Role={Role}, Duration={Duration}days",
                            servicePackage.Id, servicePackage.Name, servicePackage.AssociatedRole, servicePackage.DurationDays);

                        // Xử lý subscription và cập nhật role
                        _logger.LogInformation("🔄 Processing subscription for UserId={UserId}, PackageId={PackageId}",
                            payment.UserId, servicePackage.Id);

                        var subscription = await ProcessSubscription(payment.UserId, servicePackage);

                        _logger.LogInformation("✅ Subscription processed: StartDate={StartDate}, EndDate={EndDate}",
                            subscription.StartDate, subscription.EndDate);

                        _logger.LogInformation("🔄 Updating user roles: UserId={UserId}, Role={Role}",
                            payment.UserId, servicePackage.AssociatedRole);

                        await UpdateUserRoles(payment.UserId, servicePackage.AssociatedRole);

                        _logger.LogInformation("✅ Account upgraded for user {UserId} with role {Role}",
                            payment.UserId, servicePackage.AssociatedRole);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Service package not found: PackageId={PackageId}",
                            paymentSucceeded.ReferenceId.Value);
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ No ReferenceId found in the payment event: {@PaymentType}",
                        payment.GetType().Name);
                }

                _logger.LogDebug("🔄 Committing database transaction");
                await transaction.CommitAsync();
                _logger.LogInformation("✅ ProcessServicePackagePayment completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in ProcessServicePackagePayment: UserId={UserId}, Message={ErrorMessage}",
                    payment.UserId, ex.Message);

                _logger.LogDebug("🔄 Rolling back database transaction");
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task ProcessServicePackagePaymentDetailed(ServicePackagePaymentEvent payment)
        {
            _logger.LogInformation("🔄 Starting ProcessServicePackagePaymentDetailed: UserId={UserId}, TransactionId={TransactionId}, ServicePackageId={ServicePackageId}",
                payment.UserId, payment.TransactionId, payment.ServicePackageId);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogDebug("🔄 Database transaction started");

                // Xử lý nâng cấp tài khoản với thông tin chi tiết hơn
                Guid nonNullableServicePackageId = payment.ServicePackageId ?? Guid.Empty;

                if (nonNullableServicePackageId == Guid.Empty)
                {
                    _logger.LogError("❌ ServicePackageId is null or empty in ServicePackagePaymentEvent: TransactionId={TransactionId}",
                        payment.TransactionId);
                    throw new ArgumentException("ServicePackageId cannot be null");
                }

                _logger.LogInformation("🔍 Looking up service package: PackageId={PackageId}",
                    nonNullableServicePackageId);

                var servicePackage = await _packageRepository.GetServicePackageByIdAsync(nonNullableServicePackageId);

                if (servicePackage == null)
                {
                    _logger.LogError("❌ Service package not found: PackageId={PackageId}",
                        nonNullableServicePackageId);
                    throw new Exception($"Service package with ID {nonNullableServicePackageId} not found");
                }

                _logger.LogInformation("✅ Found service package: ID={PackageId}, Name={PackageName}, Role={Role}, Duration={Duration}days, Price={Price}",
                    servicePackage.Id, servicePackage.Name, servicePackage.AssociatedRole, servicePackage.DurationDays, servicePackage.Price);

                // Xử lý subscription
                _logger.LogInformation("🔄 Processing subscription for UserId={UserId}, PackageId={PackageId}",
                    payment.UserId, servicePackage.Id);

                var subscription = await ProcessSubscription(payment.UserId, servicePackage);

                _logger.LogInformation("✅ Subscription processed: ID={SubscriptionId}, StartDate={StartDate}, EndDate={EndDate}",
                    subscription.Id, subscription.StartDate, subscription.EndDate);

                // Cập nhật role cho user
                _logger.LogInformation("🔄 Updating user roles: UserId={UserId}, Role={Role}",
                    payment.UserId, servicePackage.AssociatedRole);

                await UpdateUserRoles(payment.UserId, servicePackage.AssociatedRole);

                _logger.LogInformation("✅ Account upgraded for user {UserId}: Role={Role}, Package={PackageName}, ExpiresOn={EndDate}",
                    payment.UserId, servicePackage.AssociatedRole, servicePackage.Name, subscription.EndDate);

                _logger.LogDebug("🔄 Committing database transaction");
                await transaction.CommitAsync();
                _logger.LogInformation("✅ ProcessServicePackagePaymentDetailed completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in ProcessServicePackagePaymentDetailed: UserId={UserId}, Message={ErrorMessage}",
                    payment.UserId, ex.Message);

                _logger.LogDebug("🔄 Rolling back database transaction");
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<ServicePackageSubscription> ProcessSubscription(Guid userId, ServicePackage package)
        {
            _logger.LogInformation("🔄 Processing subscription: UserId={UserId}, PackageId={PackageId}, PackageName={PackageName}",
                userId, package.Id, package.Name);

            var subscriptions = await _subscriptionRepository.GetSubscriptionByUserIdAsync(userId);
            _logger.LogDebug("🔍 Found {Count} existing subscriptions for UserId={UserId}",
                subscriptions.Count, userId);

            var existingSubscription = subscriptions
                .FirstOrDefault(s => s.PackageId == package.Id && s.Status == "active");

            if (existingSubscription != null)
            {
                _logger.LogInformation("🔄 Extending existing subscription: ID={SubscriptionId}, CurrentEndDate={CurrentEndDate}",
                    existingSubscription.Id, existingSubscription.EndDate);

                var oldEndDate = existingSubscription.EndDate;
                existingSubscription.EndDate = existingSubscription.EndDate.AddDays(package.DurationDays);
                existingSubscription.UpdatedAt = DateTime.UtcNow;

                _logger.LogDebug("🔄 Updating subscription: NewEndDate={NewEndDate}, ExtendedBy={Days}days",
                    existingSubscription.EndDate, package.DurationDays);

                await _subscriptionRepository.UpdateSubscriptionAsync(existingSubscription);

                _logger.LogInformation("✅ Extended subscription: ID={SubscriptionId}, From={OldEndDate} to {NewEndDate}",
                    existingSubscription.Id, oldEndDate, existingSubscription.EndDate);

                return existingSubscription;
            }

            // Tạo subscription mới
            _logger.LogInformation("🔄 Creating new subscription for UserId={UserId}, PackageId={PackageId}",
                userId, package.Id);

            var newSubscription = new ServicePackageSubscription
            {
                UserId = userId,
                PackageId = package.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(package.DurationDays),
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _logger.LogDebug("🔄 New subscription details: StartDate={StartDate}, EndDate={EndDate}, Duration={Days}days",
                newSubscription.StartDate, newSubscription.EndDate, package.DurationDays);

            await _subscriptionRepository.AddSubscriptionAsync(newSubscription);

            _logger.LogInformation("✅ Created new subscription: ID={SubscriptionId}, EndDate={EndDate}",
                newSubscription.Id, newSubscription.EndDate);

            return newSubscription;
        }

        private async Task UpdateUserRoles(Guid userId, string role)
        {
            _logger.LogInformation("🔄 Updating roles for user: UserId={UserId}, TargetRole={Role}",
                userId, role);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogError("❌ User not found: UserId={UserId}", userId);
                throw new Exception($"User {userId} not found");
            }

            _logger.LogDebug("🔍 Found user: UserId={UserId}, UserName={UserName}",
                userId, user.UserName);

            // Check if user already has the role
            var hasRole = await _userManager.IsInRoleAsync(user, role);
            _logger.LogDebug("🔍 User {UserId} already has role {Role}: {HasRole}",
                userId, role, hasRole);

            if (!hasRole)
            {
                _logger.LogInformation("🔄 Adding role {Role} to user {UserId}", role, userId);

                var result = await _userManager.AddToRoleAsync(user, role);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("❌ Failed to add role {Role} to user {UserId}: {Errors}",
                        role, userId, errors);
                    throw new Exception($"Failed to add role {role}: {errors}");
                }

                _logger.LogInformation("✅ Successfully added role {Role} to user {UserId}",
                    role, userId);
            }
            else
            {
                _logger.LogInformation("ℹ️ User {UserId} already has role {Role}, no action needed",
                    userId, role);
            }

            // Get all current roles for logging purposes
            var currentRoles = await _userManager.GetRolesAsync(user);
            _logger.LogDebug("📋 User {UserId} now has roles: {Roles}",
                userId, string.Join(", ", currentRoles));
        }
    }
}