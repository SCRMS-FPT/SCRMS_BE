using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;

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
            throw new NotFoundException($"Sport center with ID {command.SportCenterId} not found.");
        }

        sportCenter.UpdateInfo(command.Name, command.PhoneNumber, command.Description);
        var newLocation = new Location(command.AddressLine, command.City, command.District, command.Commune);
        var newGeoLocation = new GeoLocation(command.Latitude, command.Longitude);
        sportCenter.ChangeLocation(newLocation, newGeoLocation);
        var newImages = SportCenterImages.Of(command.Avatar, command.ImageUrls);
        sportCenter.ChangeImages(newImages);
        sportCenter.SetLastModified(DateTime.UtcNow);

        await _sportCenterRepository.UpdateSportCenterAsync(sportCenter, cancellationToken);

        var updatedDto = new SportCenterDetailDTO(
            Id: sportCenter.Id.Value,
            OwnerId: sportCenter.OwnerId.Value,
            Name: sportCenter.Name,
            PhoneNumber: sportCenter.PhoneNumber,
            AddressLine: sportCenter.Address.AddressLine,
            City: sportCenter.Address.City,
            District: sportCenter.Address.District,
            Commune: sportCenter.Address.Commune,
            Latitude: sportCenter.LocationPoint.Latitude,
            Longitude: sportCenter.LocationPoint.Longitude,
            Avatar: sportCenter.Images.Avatar,
            ImageUrls: sportCenter.Images.ImageUrls,
            IsDeleted: sportCenter.IsDeleted,
            Description: sportCenter.Description,
            CreatedAt: sportCenter.CreatedAt,
            LastModified: sportCenter.LastModified
        );

        return new UpdateSportCenterResult(updatedDto);
    }
}