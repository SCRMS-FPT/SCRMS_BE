using Coach.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Packages.GetActivePackages
{
    public record PackageResponse(
        Guid Id,
        Guid CoachId,
        string Name,
        string Description,
        decimal Price,
        int SessionCount,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record GetActivePackagesQuery(Guid CoachId) : IQuery<List<PackageResponse>>;

    internal class GetActivePackagesQueryHandler : IQueryHandler<GetActivePackagesQuery, List<PackageResponse>>
    {
        private readonly ICoachPackageRepository _packageRepository;

        public GetActivePackagesQueryHandler(ICoachPackageRepository packageRepository)
        {
            _packageRepository = packageRepository;
        }

        public async Task<List<PackageResponse>> Handle(GetActivePackagesQuery query, CancellationToken cancellationToken)
        {
            var packages = await _packageRepository.GetActivePackagesByCoachIdAsync(query.CoachId, cancellationToken);

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