using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CourtBooking.Application.CourtManagement.Command.CreateCourt;

public class CreateCourtHandler(IApplicationDbContext context)
    : ICommandHandler<CreateCourtCommand, CreateCourtResult>
{
    public async Task<CreateCourtResult> Handle(CreateCourtCommand command, CancellationToken cancellationToken)
    {
        var court = CreateNewCourt(command.Court);
        context.Courts.Add(court);
        await context.SaveChangesAsync(cancellationToken);
        return new CreateCourtResult(court.Id.Value);
    }

    private Court CreateNewCourt(CourtCreateDTO courtDTO)
    {
        var location = Location.Of(courtDTO.Address.Address, courtDTO.Address.Commune, courtDTO.Address.District, courtDTO.Address.City);
        //CourtId courtId, CourtName courtName, SportId sportId, Location location, string description,
        //                           string facilities, decimal pricePerHour, OwnerId ownerId
        var facilitiesJson = JsonSerializer.Serialize(courtDTO.Facilities, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        var newId = CourtId.Of(Guid.NewGuid());
        var newCourt = Court.Create(
            courtId: newId,
            courtName: CourtName.Of(courtDTO.CourtName),
            description: courtDTO.Description,
            sportId: SportId.Of(courtDTO.SportId),
            location: location,
            facilities: facilitiesJson,
            pricePerHour: courtDTO.PricePerHour,
            ownerId: OwnerId.Of(courtDTO.OwnerId)
         );
        foreach (var operatingHour in courtDTO.OperatingHours)
        {
            newCourt.AddOperatingHour( new CourtOperatingHour(
                courtId: newId,
                dayOfWeek: Int16.Parse(operatingHour.Day),
                openTime: TimeSpan.Parse(operatingHour.OpenTime),
                closeTime: TimeSpan.Parse(operatingHour.CloseTime)
            ));
        }
        return newCourt;
    }
}
