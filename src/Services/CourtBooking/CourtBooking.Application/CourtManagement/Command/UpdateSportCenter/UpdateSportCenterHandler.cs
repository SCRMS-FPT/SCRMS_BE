using BuildingBlocks.CQRS;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CourtBooking.Application.CourtManagement.Command.UpdateSportCenter;

public class UpdateSportCenterHandler : ICommandHandler<UpdateSportCenterCommand, UpdateSportCenterResult>
{
    private readonly ISportCenterRepository _sportCenterRepository;

    public UpdateSportCenterHandler(ISportCenterRepository sportCenterRepository)
    {
        _sportCenterRepository = sportCenterRepository;
    }

    public async Task<UpdateSportCenterResult> Handle(UpdateSportCenterCommand command, CancellationToken cancellationToken)
    {
        var sportCenterId = SportCenterId.Of(command.SportCenterId);
        var sportCenter = await _sportCenterRepository.GetSportCenterByIdAsync(sportCenterId, cancellationToken);
        if (sportCenter == null)
        {
            return new UpdateSportCenterResult(false);
        }

        sportCenter.UpdateInfo(command.Name, command.PhoneNumber, command.Description);
        var newLocation = new Location(command.Location.AddressLine, command.Location.City, command.Location.District, command.Location.Commune);
        var newGeoLocation = new GeoLocation(command.LocationPoint.Latitude, command.LocationPoint.Longitude);
        sportCenter.ChangeLocation(newLocation, newGeoLocation);
        var newImages = SportCenterImages.Of(command.Images.Avatar, command.Images.ImageUrls);
        sportCenter.ChangeImages(newImages);

        await _sportCenterRepository.UpdateSportCenterAsync(sportCenter, cancellationToken);
        return new UpdateSportCenterResult(true);
    }
}