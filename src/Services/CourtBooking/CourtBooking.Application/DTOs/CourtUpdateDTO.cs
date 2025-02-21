using CourtBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.DTOs;

public class CourtUpdateDTO
{
    public Guid Id { get; set; }
    public string CourtName { get; set; }
    public Guid SportId { get; set; }
    public LocationDTO Address { get; set; }
    public string Description { get; set; }
    public List<FacilityDTO> Facilities { get; set; }
    public decimal PricePerHour { get; set; }
    //public CourtStatus Status { get; set; }
    public List<CourtOperatingHourDTO> OperatingHours { get; set; }
}
