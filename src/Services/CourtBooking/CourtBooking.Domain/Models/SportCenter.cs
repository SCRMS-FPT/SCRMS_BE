using CourtBooking.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace CourtBooking.Domain.Models
{
    public class SportCenter : Aggregate<SportCenterId>
    {
        public OwnerId OwnerId { get; private set; }
        public string Name { get; private set; }
        public string PhoneNumber { get; private set; }
        public Location Address { get; private set; }
        public GeoLocation LocationPoint { get; private set; }
        public SportCenterImages Images { get; private set; }
        public string Description { get; private set; }
        public bool IsDeleted { get; private set; }

        private List<Court> _courts = new();
        public IReadOnlyCollection<Court> Courts => _courts.AsReadOnly();

        private SportCenter() { } // Required for EF Core

        private SportCenter(SportCenterId id, OwnerId ownerId, string name, string phoneNumber,
            Location address, GeoLocation location, SportCenterImages images, string description)
        {
            Id = id;
            OwnerId = ownerId;
            Name = ValidateString(name, "Name is required");
            PhoneNumber = ValidateString(phoneNumber, "Phone number is required");
            Address = address ?? throw new DomainException("Address is required");
            LocationPoint = location ?? throw new DomainException("Location is required");
            Images = images ?? new SportCenterImages(string.Empty, new List<string>());
            Description = description ?? string.Empty;
        }

        public static SportCenter Create(SportCenterId id, OwnerId ownerId, string name, string phoneNumber,
            Location address, GeoLocation location, SportCenterImages images, string description)
        {
            var newSportCenter = new SportCenter
            {
                Id = id,
                OwnerId = ownerId,
                Name = ValidateString(name, "Name is required"),
                PhoneNumber = ValidateString(phoneNumber, "Phone number is required"),
                Address = address ?? throw new DomainException("Address is required"),
                LocationPoint = location ?? throw new DomainException("Location is required"),
                Images = images ?? new SportCenterImages(string.Empty, new List<string>()),
                Description = description ?? string.Empty,
                IsDeleted = false // Explicitly set default value
            };

            //newCenter.AddDomainEvent
            return newSportCenter;
        }

        public void UpdateInfo(string name, string phoneNumber, string description)
        {
            Name = ValidateString(name, "Name is required");
            PhoneNumber = ValidateString(phoneNumber, "Phone number is required");
            Description = description ?? string.Empty;
            SetLastModified(DateTime.UtcNow);
        }

        public void ChangeLocation(Location newAddress, GeoLocation newGeoLocation)
        {
            Address = newAddress ?? throw new DomainException("New address is required");
            LocationPoint = newGeoLocation ?? throw new DomainException("New location is required");
            SetLastModified(DateTime.UtcNow);
        }

        public void ChangeImages(SportCenterImages newImages)
        {
            Images = newImages ?? throw new DomainException("Images cannot be null");
            SetLastModified(DateTime.UtcNow);
        }

        public void AddCourt(Court court)
        {
            _courts.Add(court);
        }

        public void SetIsDeleted(bool isDeleted)
        {
            IsDeleted = isDeleted;
            SetLastModified(DateTime.UtcNow);
        }

        private static string ValidateString(string value, string errorMessage)
        {
            return string.IsNullOrWhiteSpace(value) ? throw new DomainException(errorMessage) : value.Trim();
        }
    }
}

