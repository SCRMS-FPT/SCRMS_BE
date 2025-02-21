using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Domain.ValueObjects
{
    //complete this object
    public class Location
    {
        
        public string City { get; private set; }
        public string District { get; private set; }
        public string Commune { get; private set; }
        public string Address { get; private set; }

        public Location(string address, string city, string district, string commune)
        {
            Address = address;
            City = city;
            District = district;
            Commune = commune;
        }
        public static Location Of(string address, string city, string district, string commune)
        {
            return new Location(address, city, district, commune);
        }
    }
}
