using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Mapster;

namespace Identity.Application.ServicePackages.Queries.GetServicePackages
{
    public class GetServicePackagesHandler : IQueryHandler<GetServicePackagesQuery, List<ServicePackageDto>>
    {
        private readonly IServicePackageRepository _packageRepository;

        public GetServicePackagesHandler(IServicePackageRepository packageRepository)
        {
            _packageRepository = packageRepository;
        }

        public async Task<List<ServicePackageDto>> Handle(GetServicePackagesQuery query, CancellationToken cancellationToken)
        {
            var packages = await _packageRepository.GetAllServicePackageAsync();
            return packages.Select(p => p.Adapt<ServicePackageDto>()).ToList();
        }
    }
}