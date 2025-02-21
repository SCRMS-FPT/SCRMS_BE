namespace Identity.Application.Identity.Queries.ServicePackagesManagement
{
    public record GetServicePackagesQuery : IQuery<IEnumerable<ServicePackageDto>>;
}