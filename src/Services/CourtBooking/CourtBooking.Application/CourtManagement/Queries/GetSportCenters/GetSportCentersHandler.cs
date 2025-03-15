using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CourtBooking.Application.CourtManagement.Queries.GetSportCenters
{
    public class GetSportCentersHandler : IQueryHandler<GetSportCentersQuery, GetSportCentersResult>
    {
        private readonly ISportCenterRepository _sportCenterRepository;

        public GetSportCentersHandler(ISportCenterRepository sportCenterRepository)
        {
            _sportCenterRepository = sportCenterRepository;
        }

        public async Task<GetSportCentersResult> Handle(GetSportCentersQuery query, CancellationToken cancellationToken)
        {
            var pageIndex = query.PaginationRequest.PageIndex; // 0-based
            var pageSize = query.PaginationRequest.PageSize;
            var city = query.City;
            var name = query.Name;

            var totalCount = await _sportCenterRepository.GetFilteredSportCenterCountAsync(city, name, cancellationToken);
            var sportCenters = await _sportCenterRepository.GetFilteredPaginatedSportCentersAsync(
                pageIndex, pageSize, city, name, cancellationToken);

            var sportCenterDtos = sportCenters.Select(sc => new SportCenterListDTO(
                Id: sc.Id.Value,
                OwnerId: sc.OwnerId.Value,
                Name: sc.Name,
                PhoneNumber: sc.PhoneNumber,
                AddressLine: sc.Address.AddressLine,
                City: sc.Address.City,
                District: sc.Address.District,
                Commune: sc.Address.Commune,
                Latitude: sc.LocationPoint.Latitude,
                Longitude: sc.LocationPoint.Longitude,
                Avatar: sc.Images.Avatar,
                ImageUrls: sc.Images.ImageUrls,
                Description: sc.Description,
                CreatedAt: sc.CreatedAt,
                LastModified: sc.LastModified
            )).ToList();

            return new GetSportCentersResult(new PaginatedResult<SportCenterListDTO>(
                pageIndex, pageSize, totalCount, sportCenterDtos));
        }
    }
}