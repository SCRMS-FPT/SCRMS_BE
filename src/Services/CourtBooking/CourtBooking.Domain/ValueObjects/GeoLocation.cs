using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Domain.ValueObjects
{
    public class GeoLocation
    {
        public double Latitude { get; }
        public double Longitude { get; }

        private GeoLocation() { } // For EF Core

        public GeoLocation(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90)
                throw new DomainException("Invalid latitude value");

            if (longitude < -180 || longitude > 180)
                throw new DomainException("Invalid longitude value");

            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
