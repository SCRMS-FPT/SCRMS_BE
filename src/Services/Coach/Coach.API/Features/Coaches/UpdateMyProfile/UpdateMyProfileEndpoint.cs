using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Coach.API.Features.Coaches.UpdateCoach;

namespace Coach.API.Features.Coaches.UpdateMyProfile
{
    public class UpdateMyProfileEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/my-profile", async (
            [FromForm] UpdateCoachRequest request,
            [FromServices] ISender sender,
            HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                var form = await httpContext.Request.ReadFormAsync();
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachId))
                    return Results.Unauthorized();

                // We reuse the existing UpdateCoachCommand but set the CoachId from the JWT
                var command = new UpdateCoachCommand(
                    CoachId: coachId, // This comes from JWT, not from request
                    FullName: request.FullName,
                    Email: request.Email,
                    Phone: request.Phone,
                    NewAvatarFile: request.NewAvatar,
                    NewImageFiles: request.NewImages ?? form.Files.GetFiles("newImages").ToList() ?? new List<IFormFile>(),
                    ExistingImageUrls: request.ExistingImageUrls ?? new List<string>(),
                    ImagesToDelete: request.ImagesToDelete ?? new List<string>(),
                    Bio: request.Bio,
                    RatePerHour: request.RatePerHour,
                    SportIds: request.ListSport);

                var result = await sender.Send(command);
                return Results.Ok(new { Message = "Your coach profile has been updated successfully" });
            })
            .DisableAntiforgery()
            .RequireAuthorization("Coach") // Require "Coach" role instead of "Admin"
            .WithName("UpdateMyProfile")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Update My Coach Profile")
            .WithDescription("Allow coaches to update their own profile information").WithTags("Coach");
        }
    }
}