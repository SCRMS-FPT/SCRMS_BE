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
        Task<User> GetByIdAsync(Guid userId);

        Task<User> GetByEmailAsync(string email);

        Task<IdentityResult> CreateAsync(User user, string password);

        Task<IdentityResult> UpdateAsync(User user);

        Task<IdentityResult> DeleteAsync(User user);

        Task<IList<string>> GetRolesAsync(User user);

        Task<IdentityResult> AddToRoleAsync(User user, string role);

        Task<IdentityResult> RemoveFromRolesAsync(User user, IEnumerable<string> roles);

        Task<IdentityResult> AddToRolesAsync(User user, IEnumerable<string> roles);

        Task<bool> CheckPasswordAsync(User user, string password);

        Task<IdentityResult> UpdatePasswordAsync(User user, string oldPassword, string newPassword);

        Task<List<User>> GetAllAsync();
    }
}