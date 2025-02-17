using Microsoft.AspNetCore.Identity;

namespace UserAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
