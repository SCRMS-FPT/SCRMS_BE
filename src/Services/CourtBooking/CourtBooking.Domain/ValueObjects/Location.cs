using System;

namespace CourtBooking.Domain.ValueObjects
{
    public class Location
    {
        public string City { get; private set; }
        public string District { get; private set; }
        public string Commune { get; private set; }
        public string AddressLine { get; private set; }

        private Location() { } // Required for EF Core

        public Location(string addressLine, string city, string district, string commune)
        {
            if (string.IsNullOrWhiteSpace(addressLine)) throw new DomainException("Address line is required.");
            if (string.IsNullOrWhiteSpace(city)) throw new DomainException("City is required.");
            if (string.IsNullOrWhiteSpace(district)) throw new DomainException("District is required.");
            if (string.IsNullOrWhiteSpace(commune)) throw new DomainException("Commune is required.");

            AddressLine = addressLine.Trim();
            City = city.Trim();
            District = district.Trim();
            Commune = commune.Trim();
        }

        public static Location Of(string addressLine, string city, string district, string commune)
        {
            return new Location(addressLine, city, district, commune);
        }

        public override bool Equals(object obj)
        {
            if (obj is not Location other) return false;
            return AddressLine == other.AddressLine &&
                   City == other.City &&
                   District == other.District &&
                   Commune == other.Commune;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AddressLine, City, District, Commune);
        }

        public override string ToString()
        {
            return $"{AddressLine}, {Commune}, {District}, {City}";
        }
    }
}
