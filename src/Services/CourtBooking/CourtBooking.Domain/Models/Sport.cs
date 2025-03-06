using CourtBooking.Domain.ValueObjects;
using System;

namespace CourtBooking.Domain.Models
{
    public class Sport : Entity<SportId>
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Icon { get; private set; } 

        private Sport() { }
        public Sport(string name, string description, string icon)
        {
            Name = name;
            Description = description;
            Icon = icon;
        }

        public static Sport Create(SportId sportId, string name, string description, string icon)
        {
            var sport = new Sport
            {
                Id = sportId,
                Name = name,
                Description = description,
                Icon = icon,
                CreatedAt = DateTime.UtcNow
            };
            return sport;
        }

        public void Update(string name, string description, string icon)
        {
            Name = name;
            Description = description;
            Icon = icon;
            SetLastModified(DateTime.UtcNow);
        }
    }
}
