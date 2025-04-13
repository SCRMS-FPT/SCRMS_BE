using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.AspNetCore.Mvc;

namespace Coach.API.Features.Coaches.CreateCoach
{
    public class CreateCoachEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/coaches", async (
            [FromForm] CreateCoachRequest request,
            [FromServices] ISender sender,
            HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                var form = await httpContext.Request.ReadFormAsync();
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var command = new CreateCoachCommand(
                    UserId: userId,
                    FullName: request.FullName,
                    Email: request.Email,
                    Phone: request.Phone,
                    AvatarFile: request.Avatar,
                    ImageFiles: request.Images ?? form.Files.GetFiles("Images").ToList() ?? new List<IFormFile>(),
                    Bio: request.Bio,
                    RatePerHour: request.RatePerHour,
                    SportIds: new List<Guid> { request.SportId }
                );

                var result = await sender.Send(command);
                return Results.Created($"/coaches/{result.Id}", result);
            })
            .DisableAntiforgery()
            .RequireAuthorization("Coach")
            .WithName("CreateCoach")
            .Produces<CreateCoachResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Create Coach")
            .WithDescription("Create a new coach profile using authenticated user").WithTags("Coach");
        }
    }

    public class CreateCoachRequest
    {
        public Guid SportId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Bio { get; set; }
        public decimal RatePerHour { get; set; }
        public IFormFile? Avatar { get; set; }
        public List<IFormFile>? Images { get; set; }
    }

    public record CreateCoachResponse(
        Guid Id,
        string FullName,
        string AvatarUrl,
        List<string> ImageUrls,
        DateTime CreatedAt,
        List<Guid> SportIds);
}