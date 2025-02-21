using Identity.Application.Identity.Commands.Register;

namespace Identity.API.Endpoints
{
    public class Register : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/identity");

            group.MapPost("/register", async (RegisterUserRequest request, ISender sender) =>
            {
                var command = request.Adapt<RegisterUserCommand>();

                var result = await sender.Send(command);
                return Results.Created($"/api/users/{result.Id}", result);
            });
        }
    }

    public record RegisterUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateTime BirthDate,
    string Gender,
    string Password);
}