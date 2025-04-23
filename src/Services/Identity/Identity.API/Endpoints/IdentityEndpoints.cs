using Identity.Application.Identity.Commands.ChangePassword;
using Identity.Application.Identity.Commands.Login;
using Identity.Application.Identity.Commands.Register;
using Identity.Application.Identity.Commands.ResetPassword;
using Identity.Application.Identity.Commands.Role;
using Identity.Application.Identity.Commands.RefreshToken;
using Identity.Application.Identity.Commands.SigninWithGoogle;
using Identity.Application.Identity.Commands.SignupWithGoogle;
using Identity.Application.Identity.Commands.UpdateProfile;
using Identity.Application.Identity.Queries.DashboardStats;
using Identity.Application.Identity.Queries.GetProfile;
using Identity.Application.Identity.Queries.Verification;
using Identity.Domain.Exceptions;
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

            identityGroup.MapGet("/verify-email", async ([FromQuery] string Token, ISender sender) =>
            {
                await sender.Send(new VerificationQuery(Token));
                return Results.Ok();
            });

            identityGroup.MapPost("/login", async (LoginUserRequest request, ISender sender) =>
            {
                try
                {
                    var command = request.Adapt<LoginUserCommand>();
                    var result = await sender.Send(command);
                    return Results.Ok(result);
                }
                catch
                {
                    return Results.Unauthorized();
                }
            });

            identityGroup.MapPost("/loginwithgoogle", async (LoginWithGoogle request, ISender sender) =>
            {
                var command = request.Adapt<SigninCommand>();
                var result = await sender.Send(command);
                return Results.Ok(result);
            });

            identityGroup.MapPost("/register", async (RegisterUserRequest request, ISender sender) =>
            {
                var command = request.Adapt<RegisterUserCommand>();
                await sender.Send(command);
                return Results.Ok();
            });

            identityGroup.MapPost("/registerwithgoogle", async (RegisterWithGoogle request, ISender sender) =>
            {
                var command = request.Adapt<SignupCommand>();
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

            identityGroup.MapPut("/update-profile", async (
                HttpContext httpContext,
                ISender sender) =>
            {
                var form = await httpContext.Request.ReadFormAsync();

                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user id in token");

                try
                {
                    // Đọc các giá trị từ form
                    var firstName = form["FirstName"].ToString();
                    var lastName = form["LastName"].ToString();
                    var phone = form["Phone"].ToString();
                    var gender = form["Gender"].ToString();

                    DateTime birthDate;
                    if (!DateTime.TryParse(form["BirthDate"].ToString(), out birthDate))
                        return Results.BadRequest("Invalid birth date format");

                    string? selfIntroduction = form["SelfIntroduction"].ToString();

                    // Xử lý các file
                    var avatarFile = form.Files.GetFile("NewAvatarFile");
                    var imageFiles = form.Files.GetFiles("NewImageFiles").ToList();

                    // Xử lý danh sách URLs
                    List<string>? existingUrls = null;
                    if (form.TryGetValue("ExistingImageUrls", out var existingUrlValues))
                    {
                        existingUrls = existingUrlValues.ToList();
                    }

                    List<string>? imagesToDelete = null;
                    if (form.TryGetValue("ImagesToDelete", out var deleteUrlValues))
                    {
                        imagesToDelete = deleteUrlValues.ToList();
                    }

                    var command = new UpdateProfileCommand(
                        userId,
                        firstName,
                        lastName,
                        phone,
                        birthDate,
                        gender,
                        selfIntroduction,
                        avatarFile,
                        imageFiles,
                        existingUrls,
                        imagesToDelete
                    );

                    var updatedProfile = await sender.Send(command);
                    return Results.Ok(updatedProfile);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error updating profile: {ex.Message}");
                }
            })
            .RequireAuthorization()
            .DisableAntiforgery();

            identityGroup.MapPost("/users/reset-password", async (ResetPasswordRequest request, ISender sender) =>
            {
                var command = request.Adapt<RequestPasswordResetCommand>();
                await sender.Send(command);
                return Results.Ok();
            });

            identityGroup.MapPost("/users/reset-password-confirm", async (ResetPasswordConfirmRequest request, ISender sender) =>
            {
                var command = request.Adapt<ConfirmPasswordResetCommand>();
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
            }).RequireAuthorization("Admin");

            // Thêm endpoint xóa role của user dành cho Admin
            identityAdminGroup.MapPost("/remove-roles", async (RemoveRolesRequest request, ISender sender) =>
            {
                var command = new RemoveRolesFromUserCommand(request.UserId, request.Roles);
                await sender.Send(command);
                return Results.Ok(new { Message = "Roles removed successfully" });
            }).RequireAuthorization("Admin");

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
    public record VerifyEmailRequest(string Token);
    public record ChangePasswordRequest(string OldPassword, string NewPassword);
    public record LoginUserRequest(string Email, string Password);
    public record LoginWithGoogle(string Token);
    public record RegisterUserRequest(
        string FirstName,
        string LastName,
        string Email,
        string Phone,
        DateTime BirthDate,
        string Gender,
        string Password);
    public record RegisterWithGoogle(string Token,
        string Gender,
        string Phone,
        DateTime BirthDate);
    public record ResetPasswordRequest(string Email);
    public record ResetPasswordConfirmRequest(string Email, string Token, string NewPassword);
    public record AssignRolesRequest(Guid UserId, List<string> Roles);
    public record RemoveRolesRequest(Guid UserId, List<string> Roles);
    public class UpdateProfileRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? SelfIntroduction { get; set; }
        public IFormFile? NewAvatarFile { get; set; }
        public List<IFormFile>? NewImageFiles { get; set; }
        public List<string>? ExistingImageUrls { get; set; }
        public List<string>? ImagesToDelete { get; set; }
    }
}