using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.Data.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(Guid userId);

        Task<User> GetUserByEmailAsync(string email);

        Task<UserDto> GetFullUserByIdAsync(Guid userId);

        Task<UserDto> GetFullUserByEmailAsync(string email);

        Task<IdentityResult> CreateUserAsync(User user, string password);

        Task<IdentityResult> UpdateUserAsync(User user);

        Task<IdentityResult> DeleteUserAsync(User user);

        Task<IList<string>> GetRolesAsync(User user);

        Task<IdentityResult> AddToRoleAsync(User user, string role);

        Task<IdentityResult> RemoveFromRolesAsync(User user, IEnumerable<string> roles);

        Task<IdentityResult> AddToRolesAsync(User user, IEnumerable<string> roles);

        Task<bool> CheckPasswordAsync(User user, string password);

        Task<IdentityResult> UpdatePasswordAsync(User user, string oldPassword, string newPassword);

        Task<List<User>> GetAllUserAsync();
        Task<IdentityResult> CreateUserByGoogleAsync(User user);
        Task<IList<UserLoginInfo>> GetLoginsAsync(User user);
        Task<IdentityResult> AddLoginAsync(User user, UserLoginInfo info);
        Task<IdentityResult> VerifyEmailAsync(User user);
    }
}