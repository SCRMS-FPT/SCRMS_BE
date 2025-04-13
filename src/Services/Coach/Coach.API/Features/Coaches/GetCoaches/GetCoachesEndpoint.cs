using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Pagination;
using System.Security.Claims;

namespace Coach.API.Features.Coaches.GetCoaches
{
    public class GetCoachesEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/coaches", async (
                ClaimsPrincipal user,
                [FromQuery] string? name,
                [FromQuery] Guid? sportId,
                [FromQuery] decimal? minPrice,
                [FromQuery] decimal? maxPrice,
                [FromServices] ISender sender,
                [FromQuery] int pageIndex = 0,
                [FromQuery] int pageSize = 10) =>
            {
                // Lấy user ID từ claim
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid? currentUserId = string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId);

                var query = new GetCoachesQuery(name, sportId, minPrice, maxPrice, pageIndex, pageSize, currentUserId);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetCoaches")
            .Produces<PaginatedResult<CoachResponse>>()
            .WithTags("Coach")
            .WithDescription("Get all coaches with optional filtering by name, sport, and price range");
        }
    }
}