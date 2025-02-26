using Identity.Application.Identity.Commands.Register;

namespace Identity.API.Endpoints
{
    public class ResetPassword : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/identity");

            group.MapPost("/users/reset-password", async (ResetPasswordRequest request, ISender sender) =>
            {
                var command = request.Adapt<RegisterUserCommand>();

                var result = await sender.Send(command);

                return Results.Ok();
            });
        }
    }

    public record ResetPasswordRequest(
    string Email);
}
