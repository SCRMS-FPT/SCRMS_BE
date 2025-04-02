using Identity.Application.Identity.Commands.ChangePassword;
using Identity.Application.Identity.Commands.Login;
using Identity.Application.Identity.Commands.Register;
using Identity.Application.Identity.Commands.ResetPassword;
using Identity.Application.Identity.Commands.Role;
using Identity.Application.Identity.Commands.RefreshToken;
using Identity.Application.Identity.Commands.UpdateProfile;
using Identity.Application.Identity.Queries.GetProfile;
using Identity.Application.Identity.Queries.DashboardStats;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Identity.API.Endpoints
{
    public class IdentityEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Group cho các endpoint Identity (Login, Register, Change Password, Get/Update Profile, Reset Password)
            var identityGroup = app.MapGroup("/api/identity")
                                   .WithTags("Identity");

            identityGroup.MapPost("/login", async (LoginUserRequest request, ISender sender) =>
            {
                var command = request.Adapt<LoginUserCommand>();
                var result = await sender.Send(command);
                return Results.Ok(result);
            });

            identityGroup.MapPost("/register", async (RegisterUserRequest request, ISender sender) =>
            {
                var command = request.Adapt<RegisterUserCommand>();
                var result = await sender.Send(command);
                return Results.Created($"/api/users/{result.Id}", result);
            });

            identityGroup.MapPost("/change-password", async (ChangePasswordRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                  ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user id in token");

                var command = new ChangePasswordCommand(userId, request.OldPassword, request.NewPassword);
                await sender.Send(command);

                return Results.Ok(new { Message = "Password changed successfully" });
            }).RequireAuthorization();

            identityGroup.MapGet("/get-profile", async (ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                  ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user id in token");

                var query = new GetProfileQuery(userId);
                var profile = await sender.Send(query);
                return Results.Ok(profile);
            }).RequireAuthorization();

            identityGroup.MapPut("/update-profile", async (UpdateProfileRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                  ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user id in token");

                var command = new UpdateProfileCommand(
                    userId,
                    request.FirstName,
                    request.LastName,
                    request.Phone,
                    request.BirthDate,
                    request.Gender,
                    request.SelfIntroduction
                );

                var updatedProfile = await sender.Send(command);
                return Results.Ok(updatedProfile);
            }).RequireAuthorization();

            identityGroup.MapPost("/users/reset-password", async (ResetPasswordRequest request, ISender sender) =>
            {
                var command = request.Adapt<ResetPasswordCommand>();
                await sender.Send(command);
                return Results.Ok();
            });
            // Thêm endpoint trong IdentityEndpoints.cs
            identityGroup.MapPost("/refresh-token", async (ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user id in token");

                var command = new RefreshTokenCommand(userId);
                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("RefreshToken")
            .WithDescription("Refresh the JWT token to update user roles and claims.");
            // Group cho các endpoint admin về Role
            var identityAdminGroup = app.MapGroup("/api/identity/admin")
                                        .WithTags("Identity - Admin")
                                        .RequireAuthorization("Admin");

            identityAdminGroup.MapPost("/assign-roles", async (AssignRolesRequest request, ISender sender) =>
            {
                var command = new AssignRolesToUserCommand(request.UserId, request.Roles);
                await sender.Send(command);
                return Results.NoContent();
            });

            var dashboardGroup = app.MapGroup("/api/admin/dashboard/stats")
                                    .WithTags("Admin Dashboard");

            dashboardGroup.MapGet("/", async (
                ISender sender) =>
            {
                var query = new DashboardStatsQuery();
                var result = await sender.Send(query);
                return Results.Ok(result);
            }).RequireAuthorization("Admin");
        }
    }

    public record ChangePasswordRequest(string OldPassword, string NewPassword);
    public record LoginUserRequest(string Email, string Password);
    public record RegisterUserRequest(
        string FirstName,
        string LastName,
        string Email,
        string Phone,
        DateTime BirthDate,
        string Gender,
        string Password);
    public record ResetPasswordRequest(string Email);
    public record AssignRolesRequest(Guid UserId, List<string> Roles);
    public record UpdateProfileRequest(
         string FirstName,
         string LastName,
         string Phone,
         DateTime BirthDate,
         string Gender,
        string? SelfIntroduction = null
    );
}