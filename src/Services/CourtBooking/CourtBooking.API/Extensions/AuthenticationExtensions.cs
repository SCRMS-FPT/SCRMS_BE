using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

namespace CourtBooking.API.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JwtSettings:Issuer"] ?? "identity-service",
                    ValidAudience = configuration["JwtSettings:Audience"] ?? "webapp",
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"] ??
                        "8f9c08c9e6bde3fc8697fbbf91d52a5dcd2f72f84b4b8a6c7d8f3f9d3db249a1")),
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = JwtRegisteredClaimNames.Sub, // Map 'sub' claim to User.Identity.Name
                    RoleClaimType = ClaimTypes.Role
                };
            });

            return services;
        }

        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
                options.AddPolicy("CourtOwner", policy => policy.RequireRole("CourtOwner"));
                options.AddPolicy("User", policy => policy.RequireRole("User"));
                options.AddPolicy("AdminOrCourtOwner", policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRole("Admin") || context.User.IsInRole("CourtOwner")));

                // Policy cho chủ sở hữu sân với sân của chính họ
                options.AddPolicy("CourtOwnerOfCenter", policy =>
                    policy.Requirements.Add(new CourtOwnershipRequirement()));
            });

            services.AddScoped<IAuthorizationHandler, CourtOwnershipHandler>();
            return services;
        }

        // Extension method: kiểm tra xem một SportCenter có thuộc sở hữu của user hay không
        public static async Task<bool> IsSportCenterOwnedByUserAsync(this ISportCenterRepository repository, Guid sportCenterId, Guid userId, CancellationToken cancellationToken = default)
        {
            // Lấy thông tin SportCenter từ repository
            var sportCenter = await repository.GetSportCenterByIdAsync(SportCenterId.Of(sportCenterId), cancellationToken);
            return sportCenter != null && sportCenter.OwnerId.Value == userId;
        }

        // Extension method: lấy sportCenterId từ courtId
        // (Giả định bạn sẽ triển khai lại logic này dựa trên dữ liệu của Court, có thể thông qua ICourtRepository)
        public static async Task<string> GetSportCenterIdByCourtIdAsync(this ISportCenterRepository repository, Guid courtId)
        {
            // Dummy implementation: trả về chuỗi rỗng nếu không có logic cụ thể.
            // Bạn cần cài đặt lại dựa trên quan hệ giữa Court và SportCenter.
            return string.Empty;
        }
    }

    // Yêu cầu (Requirement) kiểm tra chủ sở hữu sân
    public class CourtOwnershipRequirement : IAuthorizationRequirement
    { }

    // Handler xác minh chủ sở hữu sân
    public class CourtOwnershipHandler : AuthorizationHandler<CourtOwnershipRequirement>
    {
        private readonly ISportCenterRepository _sportCenterRepository;

        public CourtOwnershipHandler(ISportCenterRepository sportCenterRepository)
        {
            _sportCenterRepository = sportCenterRepository;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CourtOwnershipRequirement requirement)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                return;
            }
            var roleClaim = context.User.FindFirst(ClaimTypes.Role);
            // Nếu user có role Admin, luôn được cấp quyền
            if (roleClaim.Value == "Admin")
            {
                context.Succeed(requirement);
                return;
            }

            if (roleClaim.Value != "CourtOwner")
            {
                return;
            }

            // Lấy HttpContext thông qua IHttpContextAccessor được truyền vào context.Resource
            var httpContextAccessor = context.Resource as IHttpContextAccessor;
            var httpContext = httpContextAccessor?.HttpContext;
            if (httpContext == null)
            {
                return;
            }

            // Lấy sportCenterId từ route (có thể đặt key là "sportCenterId" hoặc "centerId")
            var sportCenterIdValue = httpContext.Request.RouteValues["sportCenterId"] as string ??
                                      httpContext.Request.RouteValues["centerId"] as string;

            if (string.IsNullOrEmpty(sportCenterIdValue))
            {
                // Nếu không tìm thấy ID trung tâm từ route, thử lấy từ courtId
                var courtIdValue = httpContext.Request.RouteValues["courtId"] as string;
                if (!string.IsNullOrEmpty(courtIdValue) && Guid.TryParse(courtIdValue, out var parsedCourtId))
                {
                    sportCenterIdValue = await _sportCenterRepository.GetSportCenterIdByCourtIdAsync(parsedCourtId);
                }
            }

            if (string.IsNullOrEmpty(sportCenterIdValue) || !Guid.TryParse(sportCenterIdValue, out var parsedSportCenterId))
            {
                return;
            }

            // Lấy ownerId từ JWT claims (sử dụng ClaimTypes.NameIdentifier)
            var ownerIdValue = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(ownerIdValue) || !Guid.TryParse(ownerIdValue, out var parsedOwnerId))
            {
                return;
            }

            // Kiểm tra xem người dùng có phải là chủ sở hữu của trung tâm hay không
            var isOwner = await _sportCenterRepository.IsSportCenterOwnedByUserAsync(parsedSportCenterId, parsedOwnerId);
            if (isOwner)
            {
                context.Succeed(requirement);
            }
        }
    }
}