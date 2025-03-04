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
        //var location = Location.Of(courtDTO.Address.Address, courtDTO.Address.Commune, courtDTO.Address.District, courtDTO.Address.City);
        var facilitiesJson = JsonSerializer.Serialize(courtDTO.Facilities, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        //var imagesJson = JsonSerializer.Serialize(courtDTO.Images, new JsonSerializerOptions
        //{
        //    WriteIndented = true
        //});

        var newId = CourtId.Of(Guid.NewGuid());
        var newCourt = Court.Create(
            //courtId: newId,
            courtName: CourtName.Of(courtDTO.CourtName),
            sportCenterId: SportCenterId.Of(courtDTO.OwnerId),
            sportId: SportId.Of(courtDTO.SportId),
            slotDuration: TimeSpan.FromMinutes(courtDTO.SlotDuration),
            description: courtDTO.Description,
            facilities: facilitiesJson
         );
        foreach (var slot in courtDTO.CourtSlots)
        {
            newCourt.AddCourtSlot(slot.dayOfWeek, slot.startTime, slot.endTime, slot.priceSlot);
        }
        return newCourt;
    }
}
