namespace Identity.Application.Identity.Queries.ServicePackagesManagement
{
    public record GetServicePackageByIdQuery(int Id) : IQuery<ServicePackageDto?>;
}