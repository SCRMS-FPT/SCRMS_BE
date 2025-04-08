using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.ValueObjects;

namespace CourtBooking.Application.CourtManagement.Command.SoftDeleteSportCenter;

public class SoftDeleteSportCenterHandler : ICommandHandler<SoftDeleteSportCenterCommand, SoftDeleteSportCenterResult>
{
    private readonly ISportCenterRepository _sportCenterRepository;
    private readonly ICourtRepository _courtRepository;

    public SoftDeleteSportCenterHandler(ISportCenterRepository sportCenterRepository, ICourtRepository courtRepository)
    {
        _sportCenterRepository = sportCenterRepository;
        _courtRepository = courtRepository;
    }

    public async Task<SoftDeleteSportCenterResult> Handle(SoftDeleteSportCenterCommand command, CancellationToken cancellationToken)
    {
        var sportCenterId = SportCenterId.Of(command.SportCenterId);
        var sportCenter = await _sportCenterRepository.GetSportCenterByIdAsync(sportCenterId, cancellationToken);

        if (sportCenter == null)
        {
            throw new NotFoundException($"Sport center with ID {command.SportCenterId} not found.");
        }

        // Set all courts to Closed status
        var courts = await _courtRepository.GetCourtsBySportCenterIdAsync(sportCenterId, cancellationToken);
        foreach (var court in courts)
        {
            court.UpdateStatus(CourtStatus.Closed);
            await _courtRepository.UpdateCourtAsync(court, cancellationToken);
        }

        // Mark sport center as deleted (soft delete)
        await _sportCenterRepository.SoftDeleteSportCenterAsync(sportCenterId, cancellationToken);

        return new SoftDeleteSportCenterResult(true);
    }
}