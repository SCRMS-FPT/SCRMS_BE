using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;
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
        var facilitiesJson = JsonSerializer.Serialize(courtDTO.Facilities, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var newId = CourtId.Of(Guid.NewGuid());
        var newCourt = Court.Create(
            courtId: newId,
            courtName: CourtName.Of(courtDTO.CourtName),
            sportCenterId: SportCenterId.Of(courtDTO.SportCenterId),
            sportId: SportId.Of(courtDTO.SportId),
            slotDuration: courtDTO.SlotDuration,
            description: courtDTO.Description,
            facilities: facilitiesJson,
            courtType: (CourtType)courtDTO.CourtType
         );
        foreach (var slot in courtDTO.CourtSchedules)
        {
            newCourt.AddCourtSlot(newId, slot.DayOfWeek, slot.StartTime, slot.EndTime, slot.PriceSlot);
        }
        return newCourt;
    }
}
