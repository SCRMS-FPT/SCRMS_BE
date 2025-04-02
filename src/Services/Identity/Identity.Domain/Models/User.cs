using System.Text.Json;
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
        public string? SelfIntroduction { get; set; } = null!;
        public string? ImageUrls { get; set; } = "{}";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedAt { get; set; }
        public List<string> GetImageUrlsList()
        {
            if (string.IsNullOrEmpty(ImageUrls))
                return new List<string>();

            try
            {
                var imageUrlsObj = JsonSerializer.Deserialize<ImageUrlsWrapper>(ImageUrls);
                return imageUrlsObj?.Images ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public void SetImageUrlsList(List<string> urls)
        {
            var wrapper = new ImageUrlsWrapper { Images = urls ?? new List<string>() };
            ImageUrls = JsonSerializer.Serialize(wrapper);
        }

    }
    public class ImageUrlsWrapper
    {
        public List<string> Images { get; set; } = new List<string>();
    }
    // Thêm enum Gender vào namespace
    public enum Gender
    { Male, Female, Other }

    public class ServicePackageSubscription : Entity<Guid>
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!; // Navigation property
        public Guid PackageId { get; set; }
        public ServicePackage Package { get; set; } = null!; // Navigation property
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "active"; // Ví dụ: "active", "expired", "cancelled"
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}