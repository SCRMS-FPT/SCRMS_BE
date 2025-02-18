using Identity.Application.Identity.Commands.ChangePassword;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Identity.API.Endpoints
{
    public class ChangePassword : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Yêu cầu phải đăng nhập
            var group = app.MapGroup("/api/identity").RequireAuthorization();

            group.MapPost("/change-password", async (ChangePasswordRequest request, ISender sender, HttpContext httpContext) =>
            {
                // Lấy claim chứa user id (giả sử lưu trong JwtRegisteredClaimNames.Sub hoặc ClaimTypes.NameIdentifier)
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                  ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user id in token");

                var command = new ChangePasswordCommand(userId, request.OldPassword, request.NewPassword);
                await sender.Send(command);

                return Results.Ok(new { Message = "Password changed successfully" });
            });
        }
    }

    public record ChangePasswordRequest(string OldPassword, string NewPassword);

}
