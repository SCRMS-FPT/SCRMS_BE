using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;

namespace CourtBooking.Application.CourtManagement.Queries.GetSportCenterById;

public class GetSportCenterByIdHandler : IQueryHandler<GetSportCenterByIdQuery, GetSportCenterByIdResult>
{
    private readonly ISportCenterRepository _sportCenterRepository;

    public GetSportCenterByIdHandler(ISportCenterRepository sportCenterRepository)
    {
        _sportCenterRepository = sportCenterRepository;
    }

    public async Task<GetSportCenterByIdResult> Handle(GetSportCenterByIdQuery query, CancellationToken cancellationToken)
    {
        var sportCenterId = SportCenterId.Of(query.Id);
        var sportCenter = await _sportCenterRepository.GetSportCenterByIdAsync(sportCenterId, cancellationToken);
        if (sportCenter == null)
        {
            throw new NotFoundException($"Sport center with ID {query.Id} not found.");
        }

        var dto = new SportCenterDetailDTO(
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

        return new GetSportCenterByIdResult(dto);
    }
}