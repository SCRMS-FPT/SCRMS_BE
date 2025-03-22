using Identity.Application.Data.Repositories;
using Identity.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.Data;
using Microsoft.EntityFrameworkCore;
using Identity.Application.Dtos;

namespace Identity.Infrastructure.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;

        public UserRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<User> GetUserByIdAsync(Guid userId)
        {
            return await _userManager.FindByIdAsync(userId.ToString());
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<UserDto> GetFullUserByIdAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return null; // hoặc ném ngoại lệ tùy theo yêu cầu
            }

            // Lấy các vai trò của người dùng
            var roles = await _userManager.GetRolesAsync(user);

            // Trả về UserDto kèm theo các vai trò
            return new UserDto(
                Id: user.Id,
                FirstName: user.FirstName,
                LastName: user.LastName,
                Email: user.Email,
                Phone: user.PhoneNumber,
                BirthDate: user.BirthDate,
                Gender: user.Gender.ToString(), // Convert enum to string
                SelfIntroduction: user.SelfIntroduction,
                CreatedAt: user.CreatedAt,
                Roles: roles.ToList() // Chuyển danh sách vai trò thành List<string>
            );
        }

        public async Task<UserDto> GetFullUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return null; // hoặc ném ngoại lệ tùy theo yêu cầu
            }

            // Lấy các vai trò của người dùng
            var roles = await _userManager.GetRolesAsync(user);

            // Trả về UserDto kèm theo các vai trò
            return new UserDto(
                Id: user.Id,
                FirstName: user.FirstName,
                LastName: user.LastName,
                Email: user.Email,
                Phone: user.PhoneNumber,
                BirthDate: user.BirthDate,
                Gender: user.Gender.ToString(), // Convert enum to string
                SelfIntroduction: user.SelfIntroduction,
                CreatedAt: user.CreatedAt,
                Roles: roles.ToList() // Chuyển danh sách vai trò thành List<string>
            );
        }

        public async Task<IdentityResult> CreateUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> DeleteUserAsync(User user)
        {
            user.IsDeleted = true;
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IList<string>> GetRolesAsync(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<IdentityResult> AddToRoleAsync(User user, string role)
        {
            return await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<IdentityResult> RemoveFromRolesAsync(User user, IEnumerable<string> roles)
        {
            return await _userManager.RemoveFromRolesAsync(user, roles);
        }

        public async Task<IdentityResult> AddToRolesAsync(User user, IEnumerable<string> roles)
        {
            return await _userManager.AddToRolesAsync(user, roles);
        }

        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<IdentityResult> UpdatePasswordAsync(User user, string oldPassword, string newPassword)
        {
            if (!await _userManager.CheckPasswordAsync(user, oldPassword))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Mật khẩu cũ không đúng" });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<List<User>> GetAllUserAsync()
        {
            return await _userManager.Users.ToListAsync();
        }
    }
}