using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Matching.API.Features.Suggestions
{
    public class SuggestionsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/matches/suggestions", async (
                [FromServices] ISender sender, HttpContext httpContext,
                [FromQuery] int page = 1,
                [FromQuery] int limit = 10,
                [FromQuery] string? filters = null) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("ID người dùng không hợp lệ trong token");

                // Phân tích bộ lọc từ chuỗi JSON
                List<SportSkillFilter> sportSkillFilters = new();
                if (!string.IsNullOrEmpty(filters))
                {
                    try
                    {
                        sportSkillFilters = JsonSerializer.Deserialize<List<SportSkillFilter>>(filters)
                            ?? new List<SportSkillFilter>();
                    }
                    catch (JsonException)
                    {
                        return Results.BadRequest("Định dạng bộ lọc không hợp lệ");
                    }
                }

                var query = new GetSuggestionsQuery(page, limit, userId, sportSkillFilters);
                var result = await sender.Send(query);
                return Results.Ok(result);
            });
        }
    }

    public class SportSkillFilter
    {
        public Guid SportId { get; set; }
        public string SkillLevel { get; set; }
    }
}