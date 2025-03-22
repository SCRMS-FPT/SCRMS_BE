using System.Text.Json;

namespace CourtBooking.Domain.ValueObjects
{
    public class SportCenterImages
    {
        public string Avatar { get; private set; }
        public List<string> ImageUrls { get; private set; }

        private SportCenterImages() { } // Required for EF Core

        public SportCenterImages(string avatar, List<string> imageUrls)
        {
            Avatar = string.IsNullOrWhiteSpace(avatar) ? throw new DomainException("Avatar is required") : avatar;
            ImageUrls = imageUrls ?? new List<string>();
        }

        public static SportCenterImages Of(string avatar, List<string> imageUrls)
        {
            return new SportCenterImages(avatar, imageUrls);
        }

        public static SportCenterImages FromJson(string json)
        {
            var obj = JsonSerializer.Deserialize<SportCenterImages>(json);
            return obj ?? throw new DomainException("Invalid images data");
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not SportCenterImages other) return false;
            return Avatar == other.Avatar && ImageUrls.SequenceEqual(other.ImageUrls);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Avatar, ImageUrls);
        }
    }
}
