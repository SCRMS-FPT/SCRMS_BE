using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Mapster;

namespace Identity.Application.ServicePackages.Queries.ServicePackagesManagement
{
    public class GetServicePackageByIdQueryHandler : IQueryHandler<GetServicePackageByIdQuery, ServicePackageDto?>
    {
        private readonly IServicePackageRepository _packageRepository;

        public GetServicePackageByIdQueryHandler(IServicePackageRepository packageRepository)
        {
            _packageRepository = packageRepository;
        }

        public async Task<ServicePackageDto?> Handle(
            GetServicePackageByIdQuery request,
            CancellationToken cancellationToken)
        {
            var package = await _packageRepository.GetByIdAsync(request.Id);
            return package?.Adapt<ServicePackageDto>();
        }
    }
}