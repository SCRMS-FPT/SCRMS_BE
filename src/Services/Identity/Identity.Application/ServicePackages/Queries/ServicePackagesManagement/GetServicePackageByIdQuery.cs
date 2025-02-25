namespace Identity.Application.ServicePackages.Queries.ServicePackagesManagement
{
    public record GetServicePackageByIdQuery(Guid Id) : IQuery<ServicePackageDto?>;
}