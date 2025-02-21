using Microsoft.AspNetCore.Identity;

namespace Identity.Domain.Models
{
    public class User : IdentityUser<Guid>
    {
        // Thêm setter public cho các property
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public Gender Gender { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedAt { get; set; }
    }

    // Thêm enum Gender vào namespace
    public enum Gender
    { Male, Female, Other }

    public class ServicePackageSubscription : Entity<int>
    {
        public Guid UserId { get; set; }
        public int ServicePackageId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}