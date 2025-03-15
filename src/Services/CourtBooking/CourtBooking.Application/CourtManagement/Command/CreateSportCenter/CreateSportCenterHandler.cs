using BuildingBlocks.CQRS;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CourtBooking.Application.SportCenterManagement.Commands.CreateSportCenter;

public class CreateSportCenterHandler : ICommandHandler<CreateSportCenterCommand, CreateSportCenterResult>
{
    private readonly ISportCenterRepository _sportCenterRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateSportCenterHandler(ISportCenterRepository sportCenterRepository, IHttpContextAccessor httpContextAccessor)
    {
        _sportCenterRepository = sportCenterRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<CreateSportCenterResult> Handle(CreateSportCenterCommand command, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var ownerId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        var newId = SportCenterId.Of(Guid.NewGuid());
        var location = new Location(command.AddressLine, command.City, command.District, command.Commune);
        var geoLocation = new GeoLocation(command.Latitude, command.Longitude);
        var images = new SportCenterImages(command.Avatar, command.ImageUrls);

        var sportCenter = SportCenter.Create(
            id: newId,
            ownerId: OwnerId.Of(ownerId),
            name: command.Name,
            phoneNumber: command.PhoneNumber,
            address: location,
            location: geoLocation,
            images: images,
            description: command.Description
        );

        sportCenter.SetCreatedAt(DateTime.UtcNow);
        sportCenter.SetLastModified(DateTime.UtcNow);

        await _sportCenterRepository.AddSportCenterAsync(sportCenter, cancellationToken);
        return new CreateSportCenterResult(sportCenter.Id.Value);
    }
}