using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Coach.API.Data.Models
{
    public class Coach
    {
        [Key]
        public Guid UserId { get; set; }

        [MaxLength(255)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Avatar { get; set; } = string.Empty;

        public string ImageUrls { get; set; } = "{}"; // Stored as JSON string

        public string Bio { get; set; } = string.Empty;
        public decimal RatePerHour { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string Status { get; set; } = "active";
        public virtual ICollection<CoachSchedule> Schedules { get; set; }
        public virtual ICollection<CoachSport> CoachSports { get; set; }
        public virtual ICollection<CoachBooking> Bookings { get; set; }
        public virtual ICollection<CoachPackage> Packages { get; set; }
        public virtual ICollection<CoachPromotion> Promotions { get; set; }

        // Helper method to get image URLs as a list
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

        // Helper method to set image URLs from a list
        public void SetImageUrlsList(List<string> urls)
        {
            var wrapper = new ImageUrlsWrapper { Images = urls ?? new List<string>() };
            ImageUrls = JsonSerializer.Serialize(wrapper);
        }
    }

    // Helper class for JSON serialization/deserialization
    public class ImageUrlsWrapper
    {
        public List<string> Images { get; set; } = new List<string>();
    }
}