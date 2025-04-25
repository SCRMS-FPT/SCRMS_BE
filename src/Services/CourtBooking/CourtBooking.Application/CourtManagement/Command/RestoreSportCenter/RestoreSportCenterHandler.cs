using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.ValueObjects;

namespace CourtBooking.Application.CourtManagement.Command.RestoreSportCenter;

public class RestoreSportCenterHandler : ICommandHandler<RestoreSportCenterCommand, RestoreSportCenterResult>
{
    private readonly ISportCenterRepository _sportCenterRepository;
    private readonly ICourtRepository _courtRepository;

    public RestoreSportCenterHandler(ISportCenterRepository sportCenterRepository, ICourtRepository courtRepository)
    {
        _sportCenterRepository = sportCenterRepository;
        _courtRepository = courtRepository;
    }

    public async Task<RestoreSportCenterResult> Handle(RestoreSportCenterCommand command, CancellationToken cancellationToken)
    {
        var sportCenterId = SportCenterId.Of(command.SportCenterId);
        var sportCenter = await _sportCenterRepository.GetSportCenterByIdAsync(sportCenterId, cancellationToken);

        if (sportCenter == null)
        {
            throw new NotFoundException($"Sport center with ID {command.SportCenterId} not found.");
        }

        if (!sportCenter.IsDeleted)
        {
            return new RestoreSportCenterResult(false, "Sport center is not deleted and cannot be restored.");
        }

        // Restore sport center by setting IsDeleted to false
        await _sportCenterRepository.RestoreSportCenterAsync(sportCenterId, cancellationToken);

        // Reopen all courts associated with this sport center
        var courts = await _courtRepository.GetCourtsBySportCenterIdAsync(sportCenterId, cancellationToken);
        foreach (var court in courts)
        {
            // Only update courts that were closed due to deletion
            if (court.Status == CourtStatus.Closed)
            {
                court.UpdateStatus(CourtStatus.Open);
                await _courtRepository.UpdateCourtAsync(court, cancellationToken);
            }
        }

        return new RestoreSportCenterResult(true, "Sport center restored successfully.");
    }
}
