using Identity.Application.Identity.Commands.Role;

namespace Identity.API.Endpoints
{
    public class Role : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/identity/admin")
                .RequireAuthorization("Admin");

            group.MapPost("/assign-roles", async (AssignRolesRequest request, ISender sender) =>
            {
                var command = new AssignRolesToUserCommand(request.UserId, request.Roles);
                await sender.Send(command);
                return Results.NoContent();
            });
        }
    }

    public record AssignRolesRequest(Guid UserId, List<string> Roles);
}
