using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CourtBooking.Application.CourtManagement.Command.CreateCourt;

public class CreateCourtHandler : ICommandHandler<CreateCourtCommand, CreateCourtResult>
{
    private readonly ICourtRepository _courtRepository;

    public CreateCourtHandler(ICourtRepository courtRepository)
    {
        _courtRepository = courtRepository;
    }

    public async Task<CreateCourtResult> Handle(CreateCourtCommand command, CancellationToken cancellationToken)
    {
        var court = CreateNewCourt(command.Court);
        await _courtRepository.AddCourtAsync(court, cancellationToken);
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
            courtType: (CourtType)courtDTO.CourtType,
            minDepositPercentage: courtDTO.MinDepositPercentage,
            CancellationWindowHours: courtDTO.CancellationWindowHours,
            RefundPercentage: courtDTO.RefundPercentage
         );
        foreach (var slot in courtDTO.CourtSchedules)
        {
            newCourt.AddCourtSlot(newId, slot.DayOfWeek, slot.StartTime, slot.EndTime, slot.PriceSlot);
        }
        return newCourt;
    }
}