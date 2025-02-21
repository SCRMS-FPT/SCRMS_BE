namespace Identity.Application.Identity.Commands.UserManagement
{
    public record DeleteUserCommand(Guid UserId) : ICommand<Unit>;
}