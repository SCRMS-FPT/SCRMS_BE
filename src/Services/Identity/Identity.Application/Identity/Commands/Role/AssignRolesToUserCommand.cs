namespace Identity.Application.Identity.Commands.Role
{
    public record AssignRolesToUserCommand(Guid UserId, List<string> Roles) : ICommand<Unit>;
}