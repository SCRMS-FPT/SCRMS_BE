using Identity.Application.Identity.Commands.Login;

namespace Identity.API.Endpoints
{
    public class Login : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/identity");

            group.MapPost("/login", async (LoginUserRequest request, ISender sender) =>
            {
                var command = request.Adapt<LoginUserCommand>();
                var result = await sender.Send(command);
                return Results.Ok(result);
            });
        }
    }

    public record LoginUserRequest(string Email, string Password);
}