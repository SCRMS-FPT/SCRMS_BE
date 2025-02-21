using Identity.Application.Identity.Commands.UserManagement;
using Identity.Application.Identity.Queries.UserManagement;
using Identity.Domain.Models;

namespace Identity.API.Endpoints
{
    public class UserManagement : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/users").RequireAuthorization("Admin");

            group.MapDelete("/users/{userId}", async (Guid userId, ISender sender) =>
            {
                await sender.Send(new DeleteUserCommand(userId));
                return Results.NoContent();
            });
            group.MapGet("/", async (ISender sender) =>
            {
                var result = await sender.Send(new GetUsersQuery());
                return Results.Ok(result);
            });

            group.MapGet("/{id}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetUserByIdQuery(id));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPut("/{id}", async (Guid id, UpdateUserRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateUserCommand>() with { UserId = id };
                var result = await sender.Send(command);
                return Results.Ok(result);
            });
        }

        public record UpdateUserRequest(
       string FirstName,
       string LastName,
       DateTime BirthDate,
       Gender Gender
   );
    }
}