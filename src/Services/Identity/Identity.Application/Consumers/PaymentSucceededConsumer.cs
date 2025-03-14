using BuildingBlocks.Messaging.Events;
using Identity.Application.Data.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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
        }

        public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
        {
            var paymentEvent = context.Message;

            // Chỉ xử lý nếu loại thanh toán liên quan đến Identity service
            if (paymentEvent.PaymentType == "ServicePackage" ||
                paymentEvent.PaymentType == "AccountUpgrade" ||
                paymentEvent.PaymentType.StartsWith("Identity"))
            {
                _logger.LogInformation("Xử lý thanh toán gói dịch vụ: {TransactionId}", paymentEvent.TransactionId);

                // Thực hiện xử lý nâng cấp tài khoản
                await ProcessServicePackagePayment(paymentEvent);
            }
            else
            {
                _logger.LogDebug("Bỏ qua sự kiện thanh toán không phải dành cho Identity service: {PaymentType}",
                    paymentEvent.PaymentType);
            }
        }

        // Xử lý event chuyên biệt cho gói dịch vụ
        public async Task Consume(ConsumeContext<ServicePackagePaymentEvent> context)
        {
            var paymentEvent = context.Message;

            _logger.LogInformation("Xử lý thanh toán gói dịch vụ chuyên biệt: {TransactionId}, gói: {PackageType}",
                paymentEvent.TransactionId, paymentEvent.PackageType);

            // Thực hiện xử lý nâng cấp tài khoản với thông tin chi tiết hơn
            await ProcessServicePackagePaymentDetailed(paymentEvent);
        }

        private async Task ProcessServicePackagePayment(PaymentBaseEvent payment)
        {
            // Xử lý thanh toán cơ bản cho các event cũ
            // Sử dụng transaction khi cần thiết
        }

        private async Task ProcessServicePackagePaymentDetailed(ServicePackagePaymentEvent payment)
        {
            // Xử lý nâng cấp tài khoản với thông tin chi tiết hơn
            // Trong transaction
        }

        private async Task<ServicePackageSubscription> ProcessSubscription(Guid userId, ServicePackage package)
        {
            var existingSubscription = (await _subscriptionRepository.GetSubscriptionByUserIdAsync(userId))
                .FirstOrDefault(s => s.PackageId == package.Id && s.Status == "active");

            if (existingSubscription != null)
            {
                // Gia hạn subscription hiện tại
                existingSubscription.EndDate = existingSubscription.EndDate.AddDays(package.DurationDays);
                existingSubscription.UpdatedAt = DateTime.UtcNow;
                await _subscriptionRepository.UpdateSubscriptionAsync(existingSubscription);
                return existingSubscription;
            }

            // Tạo subscription mới
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

            await _subscriptionRepository.AddSubscriptionAsync(newSubscription);
            return newSubscription;
        }

        private async Task UpdateUserRoles(Guid userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogError("User not found: {UserId}", userId);
                throw new Exception($"User {userId} not found");
            }

            if (!await _userManager.IsInRoleAsync(user, role))
            {
                var result = await _userManager.AddToRoleAsync(user, role);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to add role {Role} to user {UserId}: {Errors}", role, userId, errors);
                    throw new Exception($"Failed to add role: {errors}");
                }
            }
        }
    }
}