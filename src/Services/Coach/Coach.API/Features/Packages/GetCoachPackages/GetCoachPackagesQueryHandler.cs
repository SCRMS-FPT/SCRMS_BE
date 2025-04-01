using Coach.API.Data.Repositories;
using Coach.API.Features.Packages.GetActivePackages;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Packages.GetCoachPackages
{
    public record GetCoachPackagesQuery(Guid CoachId) : IQuery<List<PackageResponse>>;

    internal class GetCoachPackagesQueryHandler : IQueryHandler<GetCoachPackagesQuery, List<PackageResponse>>
    {
        private readonly ICoachPackageRepository _packageRepository;

        public GetCoachPackagesQueryHandler(ICoachPackageRepository packageRepository)
        {
            _packageRepository = packageRepository;
        }

        public async Task<List<PackageResponse>> Handle(GetCoachPackagesQuery query, CancellationToken cancellationToken)
        {
            var packages = await _packageRepository.GetAllPackagesByCoachIdAsync(query.CoachId, cancellationToken);

            return packages.Select(p => new PackageResponse(
                p.Id,
                p.CoachId,
                p.Name,
                p.Description,
                p.Price,
                p.SessionCount,
                p.Status,
                p.CreatedAt,
                p.UpdatedAt
            )).ToList();
        }
    }
}