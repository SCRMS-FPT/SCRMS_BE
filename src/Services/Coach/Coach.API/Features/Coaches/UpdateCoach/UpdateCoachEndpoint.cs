using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Coach.API.Features.Coaches.UpdateCoach
{
    public class UpdateCoachEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/coach/{coachId:guid}", async (
            [FromForm] UpdateCoachRequest request,
            [FromRoute] Guid coachId,
            [FromServices] ISender sender,
            HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                var form = await httpContext.Request.ReadFormAsync();
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var command = new UpdateCoachCommand(
                    CoachId: coachId,
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
                return Results.Ok(new { Message = "Coach profile updated successfully" });
            })
            .DisableAntiforgery()
            .RequireAuthorization("Admin")
            .WithName("UpdateCoach")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Update Coach")
            .WithDescription("Update coach profile").WithTags("Coach");
        }
    }

    public class UpdateCoachRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Bio { get; set; }
        public decimal RatePerHour { get; set; }
        public List<Guid> ListSport { get; set; }

        // Phần ảnh
        public IFormFile? NewAvatar { get; set; }
        public List<IFormFile>? NewImages { get; set; }
        public List<string>? ExistingImageUrls { get; set; }
        public List<string>? ImagesToDelete { get; set; }
    }
}