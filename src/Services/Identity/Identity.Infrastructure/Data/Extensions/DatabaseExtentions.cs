using Identity.Domain.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure.Data.Extensions
{
    public static class DatabaseExtentions
    {
        public static async Task InitialiseDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            await context.Database.MigrateAsync();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            await SeedRolesAsync(roleManager);
            await SeedAdminUserAsync(userManager);
            await SeedAdditionalUsersAsync(userManager);
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
            await SeedUserAsync(userManager, "courtowner@gmail.com", "CourtOwner", "CourtOwner123!");
            await SeedUserAsync(userManager, "user@gmail.com", null, "User123!");
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
    }
}