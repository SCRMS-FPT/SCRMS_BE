using Identity.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.ServicePackages.Commands.SubscribeToServicePackage
{
    public class SubscribeToServicePackageHandler : ICommandHandler<SubscribeToServicePackageCommand, SubscribeToServicePackageResult>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;

        public SubscribeToServicePackageHandler(IApplicationDbContext dbContext, UserManager<User> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<SubscribeToServicePackageResult> Handle(SubscribeToServicePackageCommand command, CancellationToken cancellationToken)
        {
            // Tìm gói dịch vụ
            var package = await _dbContext.ServicePackages.FindAsync(command.PackageId);
            if (package == null)
                throw new DomainException("Service package not found");

            // Kiểm tra trạng thái gói dịch vụ
            if (package.Status != "active")
                throw new DomainException("Cannot subscribe to an inactive service package");

            // Tìm người dùng
            var user = await _userManager.FindByIdAsync(command.UserId.ToString());
            if (user == null)
                throw new DomainException("User not found");

            // Tạo đăng ký gói dịch vụ
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(package.DurationDays);

            var subscription = new ServicePackageSubscription
            {
                UserId = command.UserId,
                PackageId = command.PackageId,
                StartDate = startDate,
                EndDate = endDate,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Subscriptions.Add(subscription);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Gán vai trò nếu chưa có
            if (!await _userManager.IsInRoleAsync(user, package.AssociatedRole))
            {
                var result = await _userManager.AddToRoleAsync(user, package.AssociatedRole);
                if (!result.Succeeded)
                    throw new DomainException($"Failed to assign role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return new SubscribeToServicePackageResult(
                subscription.Id,
                subscription.PackageId,
                subscription.StartDate,
                subscription.EndDate,
                subscription.Status,
                package.AssociatedRole
            );
        }
    }
}