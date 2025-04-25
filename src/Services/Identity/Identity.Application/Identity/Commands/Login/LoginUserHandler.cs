using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Application.Identity.Commands.Login
{
    public sealed class LoginUserHandler : ICommandHandler<LoginUserCommand, LoginUserResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<LoginUserHandler> _logger;

        public LoginUserHandler(
            IUserRepository userRepository,
            ISubscriptionRepository subscriptionRepository,
            IOptions<JwtSettings> jwtSettings,
            ILogger<LoginUserHandler> logger)
        {
            _userRepository = userRepository;
            _subscriptionRepository = subscriptionRepository;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<LoginUserResult> Handle(
            LoginUserCommand command,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByEmailAsync(command.Email);

            if (user == null || !await _userRepository.CheckPasswordAsync(user, command.Password))
            {
                throw new DomainException("Invalid credentials");
            }
            if (user.IsDeleted)
            {
                throw new DomainException("Invalid credentials");
            }

            // Check for expired subscriptions and update roles accordingly
            await CheckAndUpdateExpiredSubscriptions(user);

            var token = await GenerateJwtToken(user);
            var roles = await _userRepository.GetRolesAsync(user);
            var userDto = new UserDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.BirthDate,
                user.Gender.ToString(),
                user.SelfIntroduction,
                user.CreatedAt,
                roles.ToList(),
                user.GetImageUrlsList()
            );
            return new LoginUserResult(
                Token: token,
                UserId: user.Id,
                User: userDto
            );
        }

        private async Task CheckAndUpdateExpiredSubscriptions(User user)
        {
            _logger.LogInformation("🔍 Checking for expired subscriptions for user: UserId={UserId}", user.Id);

            var subscriptions = await _subscriptionRepository.GetSubscriptionByUserIdAsync(user.Id);
            var now = DateTime.UtcNow;
            var expiredSubscriptions = subscriptions
                .Where(s => s.Status == "active" && s.EndDate < now)
                .ToList();

            if (!expiredSubscriptions.Any())
            {
                _logger.LogDebug("✅ No expired subscriptions found for user: UserId={UserId}", user.Id);
                return;
            }

            _logger.LogInformation("⚠️ Found {Count} expired subscriptions for user: UserId={UserId}",
                expiredSubscriptions.Count, user.Id);

            // Group by the associated role to check if all subscriptions for a role are expired
            var subscriptionsByPackage = subscriptions
                .Where(s => s.Status == "active")
                .GroupBy(s => s.Package?.AssociatedRole)
                .Where(g => g.Key != null)
                .ToDictionary(g => g.Key!, g => g.ToList());

            // Track roles that need to be removed
            var rolesToRemove = new List<string>();

            // Update expired subscriptions status
            foreach (var subscription in expiredSubscriptions)
            {
                _logger.LogInformation("🔄 Marking subscription as expired: ID={SubscriptionId}, PackageId={PackageId}, EndDate={EndDate}",
                    subscription.Id, subscription.PackageId, subscription.EndDate);

                subscription.Status = "expired";
                subscription.UpdatedAt = now;
                await _subscriptionRepository.UpdateSubscriptionAsync(subscription);

                // Check if this was the last active subscription for its role
                var associatedRole = subscription.Package?.AssociatedRole;
                if (associatedRole != null)
                {
                    var packagesForRole = subscriptionsByPackage.GetValueOrDefault(associatedRole);
                    if (packagesForRole != null)
                    {
                        var hasActiveSubscription = packagesForRole.Any(s =>
                            s.Id != subscription.Id && s.Status == "active" && s.EndDate >= now);

                        if (!hasActiveSubscription && !rolesToRemove.Contains(associatedRole))
                        {
                            _logger.LogInformation("⚠️ No active subscriptions left for role: {Role}, will remove from user",
                                associatedRole);
                            rolesToRemove.Add(associatedRole);
                        }
                    }
                }
            }

            // Remove roles that no longer have active subscriptions
            if (rolesToRemove.Any())
            {
                _logger.LogInformation("🔄 Removing roles from user {UserId}: {Roles}",
                    user.Id, string.Join(", ", rolesToRemove));

                var result = await _userRepository.RemoveFromRolesAsync(user, rolesToRemove);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("❌ Failed to remove roles: {Errors}", errors);
                    throw new Exception($"Failed to update user roles: {errors}");
                }

                _logger.LogInformation("✅ Successfully removed expired roles from user: UserId={UserId}", user.Id);
            }
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var roles = await _userRepository.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpiryHours),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}