namespace Identity.Application.Identity.Commands.ServicePackagesManagement
{
    public record DeleteServicePackageCommand(int Id) : ICommand<Unit>;
}