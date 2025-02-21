using CourtBooking.Domain.ValueObjects;
using System;

namespace CourtBooking.Domain.Models
{
    public class Sport : Entity<SportId>
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        private Sport() { } // For EF Core
        public Sport(string name, string description)
        {
            Name = name;
            Description = description;
        }


        public static Sport Create(SportId sportId,string name, string description)
        {
            var sport = new Sport
            {
                Id = sportId,
                Name = name,
                Description = description
            };
            return sport;
        }
        public static Sport Of(string name, string description)
        {
            return new Sport(name, description);
        }
    }
}
