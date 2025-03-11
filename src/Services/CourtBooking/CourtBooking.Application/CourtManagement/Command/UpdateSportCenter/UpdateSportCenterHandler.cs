using BuildingBlocks.CQRS;
using CourtBooking.Domain.Models;
using Microsoft.EntityFrameworkCore;

public class UpdateSportCenterHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateSportCenterCommand, UpdateSportCenterResult>
{
    public async Task<UpdateSportCenterResult> Handle(UpdateSportCenterCommand command, CancellationToken cancellationToken)
    {
        var sportCenterId = SportCenterId.Of(command.SportCenterId);

        var sportCenter = await context.SportCenters
            .FirstOrDefaultAsync(sc => sc.Id == sportCenterId, cancellationToken);

        if (sportCenter == null)
        {
            // Handle not found case, or throw an exception
            return new UpdateSportCenterResult(false);
        }

        // Update SportCenter fields
        sportCenter.UpdateInfo(
            command.Name,
            command.PhoneNumber,
            command.Description
        );

        // Update Location
        var newLocation = new Location(
            command.Location.AddressLine,
            command.Location.City,
            command.Location.District,
            command.Location.Commune
        );
        var newGeoLocation = new GeoLocation(
            command.LocationPoint.Latitude,
            command.LocationPoint.Longitude
        );
        sportCenter.ChangeLocation(newLocation, newGeoLocation);

        // Update Images
        var newImages = SportCenterImages.Of(
            command.Images.Avatar,
            command.Images.ImageUrls
        );
        sportCenter.ChangeImages(newImages);

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateSportCenterResult(true);
    }
}
