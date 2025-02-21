using Mapster;

namespace Identity.Application.Identity.Queries.ServicePackagesManagement
{
    public class GetServicePackageByIdQueryHandler(
        IApplicationDbContext context)
        : IQueryHandler<GetServicePackageByIdQuery, ServicePackageDto?>
    {
        public async Task<ServicePackageDto?> Handle(
            GetServicePackageByIdQuery request,
            CancellationToken cancellationToken)
        {
            return await context.ServicePackages
                .AsNoTracking()
                .Where(x => x.Id == request.Id)
                .ProjectToType<ServicePackageDto>()
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}