using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.ValueObjects;

namespace CourtBooking.Application.CourtManagement.Command.DeleteSportCenter;

public class DeleteSportCenterHandler : ICommandHandler<DeleteSportCenterCommand, DeleteSportCenterResult>
{
    private readonly ISportCenterRepository _sportCenterRepository;
    private readonly ICourtRepository _courtRepository;

    public DeleteSportCenterHandler(ISportCenterRepository sportCenterRepository, ICourtRepository courtRepository)
    {
        _sportCenterRepository = sportCenterRepository;
        _courtRepository = courtRepository;
    }

    public async Task<DeleteSportCenterResult> Handle(DeleteSportCenterCommand command, CancellationToken cancellationToken)
    {
        var sportCenterId = SportCenterId.Of(command.SportCenterId);
        var sportCenter = await _sportCenterRepository.GetSportCenterByIdAsync(sportCenterId, cancellationToken);

        if (sportCenter == null)
        {
            throw new NotFoundException($"Sport center with ID {command.SportCenterId} not found.");
        }

        // Delete all courts associated with this sport center
        var courts = await _courtRepository.GetCourtsBySportCenterIdAsync(sportCenterId, cancellationToken);
        foreach (var court in courts)
        {
            await _courtRepository.DeleteCourtAsync(court.Id, cancellationToken);
        }

        // Delete the sport center
        await _sportCenterRepository.DeleteSportCenterAsync(sportCenterId, cancellationToken);

        return new DeleteSportCenterResult(true);
    }
}