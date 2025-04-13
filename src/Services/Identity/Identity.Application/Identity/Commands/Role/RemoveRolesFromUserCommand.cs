namespace Identity.Application.Identity.Commands.Role
{
    public record RemoveRolesFromUserCommand(Guid UserId, List<string> Roles) : ICommand<Unit>;
}