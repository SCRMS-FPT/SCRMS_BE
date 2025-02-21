using CourtBooking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.Extensions;

public static class CourtExtension
{
    public static IEnumerable<CourtDTO> ToCourtDtoList(this IEnumerable<Court> courts)
    {
        return courts.Select(court => new CourtDTO
        (
            Id: court.Id.Value,
            CourtName: court.CourtName.Value,
            Description: court.Description,
            OwnerId: court.OwnerId.Value,
            Sport: new SportDTO
            (
                Name: court.Sport.Name,
                Description: court.Sport.Description
            ),
            Price: court.PricePerHour,
            CourtOperatingHours: court.OperatingHours.Select(courtOperatingHour => new CourtOperatingHourDTO
            (
                Day: courtOperatingHour.DayOfWeek.ToString(),
                OpenTime: courtOperatingHour.OpenTime.ToString(),
                CloseTime: courtOperatingHour.CloseTime.ToString()
            )).ToList(),
            Address: new LocationDTO
            (
                Address: court.Location.Address,
                Commune: court.Location.Commune,
                District: court.Location.District,
                City: court.Location.City
            )

        ));
    }
}
