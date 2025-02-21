using Identity.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Data.Managers
{
    public class SoftDeleteUserManager : UserManager<User>
    {
        public SoftDeleteUserManager(
            IUserStore<User> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<User> passwordHasher,
            IEnumerable<IUserValidator<User>> userValidators,
            IEnumerable<IPasswordValidator<User>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<User>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public override async Task<IdentityResult> DeleteAsync(User user)
        {
            user.IsDeleted = true;
            return await UpdateAsync(user);
        }
    }
}