namespace Identity.Application.ServicePackages.Commands.ServicePackagesManagement
{
    public record DeleteServicePackageCommand(
        Guid Id
    ) : ICommand<Unit>;
}