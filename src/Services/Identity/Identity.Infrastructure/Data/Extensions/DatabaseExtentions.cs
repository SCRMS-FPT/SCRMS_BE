using Identity.Domain.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.Data.Extensions
{
    public static class DatabaseExtentions
    {
        public static async Task InitialiseDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<IdentityDbContext>>();
            var isDatabaseCreated = await context.Database.CanConnectAsync();
            if (!isDatabaseCreated)
            {
                await context.Database.MigrateAsync();
            }

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            await SeedRolesAsync(roleManager);
            await SeedAdminUserAsync(userManager);
            await SeedAdditionalUsersAsync(userManager);
            await SeedServicePackagesAsync(context, logger);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            string[] roleNames = { "Admin", "CourtOwner", "Coach" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<User> userManager)
        {
            var adminEmail = "admin@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Thang",
                    LastName = "Admin",
                    BirthDate = new DateTime(2003, 5, 8, 0, 0, 0, DateTimeKind.Utc),
                    Gender = Gender.Male,
                    IsDeleted = false
                };
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        private static async Task SeedAdditionalUsersAsync(UserManager<User> userManager)
        {
            await SeedUserAsync(userManager, "coach@gmail.com", "Coach", "Coach123!");
            await SeedCourtOwnerAsync(userManager, "courtowner@gmail.com", "CourtOwner", "CourtOwner123!");
            await SeedUserAsync(userManager, "user@gmail.com", null, "User123!");
        }

        private static async Task SeedCourtOwnerAsync(UserManager<User> userManager, string email, string role, string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            Guid CourtOwnerUserId = new Guid("8e445865-a24d-4543-a6c6-9443d048cdb9");
            if (user == null)
            {
                user = new User
                {
                    Id = CourtOwnerUserId,
                    UserName = email,
                    Email = email,
                    FirstName = "OwnerFirstName",
                    LastName = "OwnerLastName",
                    BirthDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Gender = Gender.Male,
                    IsDeleted = false
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded && role != null)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }

        private static async Task SeedUserAsync(UserManager<User> userManager, string email, string role, string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    FirstName = "FirstName",
                    LastName = "LastName",
                    BirthDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Gender = Gender.Male,
                    IsDeleted = false
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded && role != null)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }

        private static async Task SeedServicePackagesAsync(IdentityDbContext context, ILogger logger)
        {
            logger.LogInformation("Seeding service packages...");

            // Premium Coach Package
            var coachPackageName = "Gói Huấn Luyện Viên Cao Cấp";
            if (!await context.ServicePackages.AnyAsync(p => p.Name == coachPackageName))
            {
                logger.LogInformation("Thêm {PackageName} vào cơ sở dữ liệu", coachPackageName);
                var coachPackage = ServicePackage.Create(
                    coachPackageName,
                    "Trở thành huấn luyện viên được xác minh với khả năng hiển thị và tính năng đặt lịch nâng cao. Bao gồm xác minh hồ sơ, ưu tiên hiển thị trong kết quả tìm kiếm và công cụ lập lịch nâng cao.",
                    299000, // 299,000 VND
                    30,
                    "Coach",
                    "active"
                );

                context.ServicePackages.Add(coachPackage);
            }

            // Gói Chủ Sân
            var courtPackageName = "Gói Quản Lý Sân";
            if (!await context.ServicePackages.AnyAsync(p => p.Name == courtPackageName))
            {
                logger.LogInformation("Thêm {PackageName} vào cơ sở dữ liệu", courtPackageName);
                var courtPackage = ServicePackage.Create(
                    courtPackageName,
                    "Giải pháp toàn diện cho chủ sân. Bao gồm quản lý danh sách sân, lịch đặt sân, xử lý thanh toán và bảng điều khiển phân tích cho địa điểm của bạn.",
                    599000, // 599,000 VND
                    365,
                    "CourtOwner",
                    "active"
                );

                context.ServicePackages.Add(courtPackage);
            }

            // Gói Chủ Sân Hàng Năm
            var annualCourtPackageName = "Gói Quản Lý Sân Hàng Năm";
            if (!await context.ServicePackages.AnyAsync(p => p.Name == annualCourtPackageName))
            {
                logger.LogInformation("Thêm {PackageName} vào cơ sở dữ liệu", annualCourtPackageName);
                var courtAnnualPackage = ServicePackage.Create(
                    annualCourtPackageName,
                    "Gói giá trị tốt nhất cho chủ sân. Tất cả các tính năng của gói tiêu chuẩn cộng với hỗ trợ ưu tiên và phân tích đặt sân nâng cao. Tiết kiệm với thanh toán hàng năm.",
                    5990000, // 5,990,000 VND
                    365,
                    "CourtOwner",
                    "active"
                );

                context.ServicePackages.Add(courtAnnualPackage);
            }

            // Save all changes at once
            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Service packages seeded successfully");
            }
            else
            {
                logger.LogInformation("All service packages already exist, no changes made");
            }
        }
    }
}